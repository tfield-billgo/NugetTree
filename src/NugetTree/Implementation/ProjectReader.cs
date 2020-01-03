using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using NugetTree.Core;
using NugetTree.Models;

namespace NugetTree.Implementation
{
    public class ProjectReader : IProjectReader
    {
        private const string INCLUDE = "Include";
        private const string ITEM_GROUP = "ItemGroup";
        private const string PACKAGE_REFERENCE = "PackageReference";
        private const string PROPERTY_GROUP = "PropertyGroup";
        private const string TARGET_FRAMEWORK = "TargetFramework";
        private const string TARGET_FRAMEWORK_VERSION = "TargetFrameworkVersion";
        private const string VERSION = "Version";

        public Task<ProjectDependencyInfo> ReadProjectFile(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentNullException(nameof(filename));

            if (!File.Exists(filename))
                throw new ArgumentException($"Project does not exist: {filename}", nameof(filename));

            var projectName = filename.Substring(filename.LastIndexOf("\\") + 1);

            var fileContents = File.ReadAllText(filename);
            var xel = XElement.Parse(fileContents);

            // <Project>
            //   <ItemGroup>
            //     <PackageReference />
            //   </ItemGroup>
            // </Project>
            var referenceElements = xel.Elements()
                .Where(el => el.Name.LocalName == ITEM_GROUP)
                .SelectMany(g => g.Elements().Where(gel => gel.Name.LocalName == PACKAGE_REFERENCE));

            // <Project>
            //   <PropertyGroup>
            //     <TargetFramework />        ==> netstandard, netcoreapp
            //     <TargetFrameworkVersion /> ==> v4.5, v4.7.2, ...
            //   </PropertyGroup>
            // </Project>
            var frameworkEl = xel.Elements()
                .Where(el => el.Name.LocalName == PROPERTY_GROUP)
                .SelectMany(el => el.Elements().Where(pel => pel.Name.LocalName == TARGET_FRAMEWORK)
                .Concat(el.Elements().Where(cel => cel.Name.LocalName == TARGET_FRAMEWORK_VERSION)))
                .FirstOrDefault();

            var frameworkName = frameworkEl?.Value;

            var dependencies = referenceElements.Select(element =>
            {
                // <PackageReference Include="Microsoft.Extensions.Logging.Abstractions">
                //     <Version>3.0.1</Version>
                // </PackageReference>
                // or:
                // <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="3.0.1" />
                var packageId = element.Attributes().FirstOrDefault(a => a.Name.LocalName == INCLUDE)?.Value;
                var versionId = element.Attributes().FirstOrDefault(a => a.Name.LocalName == VERSION)?.Value
                    ?? element.Elements().FirstOrDefault(el => el.Name.LocalName == VERSION)?.Value
                    ?? string.Empty;

                return new Dependency
                {
                    Name = packageId,
                    Version = versionId,
                    Framework = frameworkName,
                    Project = projectName
                };
            });

            var project = new ProjectDependencyInfo()
            {
                FrameworkVersion = frameworkName ?? "Unknown",
                ProjectName = projectName,
                Dependencies = dependencies.ToList()
            };

            return Task.FromResult(project);
        }
    }
}