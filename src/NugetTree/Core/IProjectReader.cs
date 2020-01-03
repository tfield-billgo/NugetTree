using System.Threading.Tasks;
using NugetTree.Models;

namespace NugetTree.Core
{
    public interface IProjectReader
    {
        Task<ProjectDependencyInfo> ReadProjectFile(string filename);
    }
}