using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NugetTree.Core;
using NugetTree.Models;

namespace NugetTree.Implementation
{
    public class ProjectReaderNugetDependencyDecorator : IProjectReader
    {
        private const bool INCLUDE_PRERELEASE = false;
        private const string PACKAGES_CONFIG_FILENAME = "packages.config";

        private readonly IProjectReader _base;
        private readonly SourceCacheContext _cacheContext;
        private readonly CancellationToken _cancellationToken;
        private readonly DependencyInfoResource _dependencyResolver;
        private readonly ILogger _logger;
        private readonly PackageMetadataResource _metadataProvider;

        public ProjectReaderNugetDependencyDecorator(IProjectReader @base,
            PackageMetadataResource metadataProvider,
            DependencyInfoResource dependencyResolver,
            SourceCacheContext cacheContext,
            ILogger logger)
        {
            _base = @base ?? throw new ArgumentNullException(nameof(@base));
            _metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
            _dependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
            _cacheContext = cacheContext ?? throw new ArgumentNullException(nameof(cacheContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _cancellationToken = CancellationToken.None;
        }

        public async Task<ProjectDependencyInfo> ReadProjectFile(string filename)
        {
            var project = await _base.ReadProjectFile(filename);
            if (project == null)
                return null;

            var packagesConfigPath = filename.Replace(project.ProjectName, PACKAGES_CONFIG_FILENAME);

            PopulateDependenciesFromPackagesConfig(project, packagesConfigPath);

            foreach (var dependency in project.Dependencies)
            {
                await PopulateDependencies(dependency);
            }

            return project;
        }

        private async Task PopulateDependencies(Dependency dependency)
        {
            var dependencies = new List<Dependency>();

            NuGetVersion.TryParse(dependency.Version, out var version);
            var package = new PackageIdentity(dependency.Name, version);
            var framework = String.IsNullOrEmpty(dependency.Framework)
                ? NuGetFramework.AnyFramework
                : NuGetFramework.ParseFrameworkName(dependency.Framework, new DefaultFrameworkNameProvider());

            var dependencyInfo = await _dependencyResolver.ResolvePackage(package, framework, _cacheContext, _logger, _cancellationToken);

            if (dependencyInfo == null)
            {
                _logger.LogWarning($"Package not available in source(s): {dependency.Name} {dependency.Version}. Required by {dependency.Project}");
                return;
            }

            dependency.FoundInSources = true;

            if (dependencyInfo?.Dependencies == null)
                return;

            foreach (var info in dependencyInfo.Dependencies)
            {
                var searchResults = await _metadataProvider.GetMetadataAsync(info.Id, includePrerelease: INCLUDE_PRERELEASE, includeUnlisted: false, sourceCacheContext: _cacheContext, _logger, _cancellationToken);

                var metadata = searchResults.OrderBy(s => s.Identity.Version).LastOrDefault(r => info.VersionRange.Satisfies(r.Identity.Version));

                if (metadata == null)
                {
                    _logger.LogWarning($"Package not available in source(s): {info.Id} {info.VersionRange}.  Required by {dependency.Name} from {dependency.Project}");
                    continue;
                }

                var subpackage = await _dependencyResolver.ResolvePackage(metadata.Identity, framework, _cacheContext, _logger, _cancellationToken);
                var childDependency = new Dependency
                {
                    Framework = dependency.Framework,
                    Name = subpackage.Id,
                    Version = subpackage.Version?.Version?.ToString(),
                    Project = dependency.Project,
                    VersionLimited = info.VersionRange.MaxVersion == null ? null : $"{info.VersionRange.MaxVersion.Version.ToString()} by {dependency.Name} ({dependency.Version})"
                };

                dependencies.Add(childDependency);

                await PopulateDependencies(childDependency);
            }

            dependency.Dependencies = dependencies;
        }

        private void PopulateDependenciesFromPackagesConfig(ProjectDependencyInfo project, string packagesConfigPath)
        {
            if (!File.Exists(packagesConfigPath))
                return;

            var fileContent = File.ReadAllText(packagesConfigPath);

            var xel = XElement.Parse(fileContent);
            var packages = xel.Elements()
                .Where(l => l.Name.LocalName == "package")
                .Select(el =>
                {
                    var packageId = el.Attributes().FirstOrDefault(a => a.Name.LocalName == "id")?.Value;
                    var packageVersion = el.Attributes().FirstOrDefault(a => a.Name.LocalName == "version")?.Value;

                    return new Dependency
                    {
                        Framework = project.FrameworkVersion,
                        Name = packageId,
                        Project = project.ProjectName,
                        Version = packageVersion,
                    };
                });

            project.Dependencies = project.Dependencies.Concat(packages);
        }
    }
}