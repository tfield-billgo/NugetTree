using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NugetTree.Core;
using NugetTree.Implementation;

namespace NugetTree
{
    public class DependencyProcessorFactory
    {
        private const string PACKAGE_SOURCE = "https://api.nuget.org/v3/index.json";

        public static async Task<IDependencyProcessor> CreateDependencyProcessorAsync(LogLevel logLevel)
        {
            var reader = await CreateProjectReaderAsync(logLevel);
            var writer = CreateProjectWriter();

            return new DependencyProcessor(reader, writer);
        }

        private static async Task<IProjectReader> CreateProjectReaderAsync(LogLevel logLevel)
        {
            var baseReader = new ProjectReader();

            var v3Providers = Repository.Provider.GetCoreV3();
            var v3PackageSource = new PackageSource(PACKAGE_SOURCE);

            var v3Repository = new SourceRepository(v3PackageSource, v3Providers);

            var metadataResource = await v3Repository.GetResourceAsync<PackageMetadataResource>();
            var dependencyResource = await v3Repository.GetResourceAsync<DependencyInfoResource>();

            var cacheContext = new SourceCacheContext();
            var logger = new ConsoleLogger(logLevel);

            return new ProjectReaderNugetDependencyDecorator(
                baseReader,
                metadataResource,
                dependencyResource,
                cacheContext,
                logger);
        }

        private static IProjectWriter CreateProjectWriter()
        {
            return new ProjectWriter();
        }
    }
}