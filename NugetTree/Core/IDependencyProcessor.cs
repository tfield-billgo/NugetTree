using System.Collections.Generic;
using System.Threading.Tasks;
using NugetTree.Models;

namespace NugetTree.Core
{
    public interface IDependencyProcessor
    {
        Task<IEnumerable<ProjectDependencyInfo>> ProcessSolution(string solutionFileName, params OutputType[] outputTypes);
    }
}