using System.Collections.Generic;
using NugetTree.Models;

namespace NugetTree.Core
{
    public interface IProjectWriter
    {
        void WriteProjects(IEnumerable<ProjectDependencyInfo> projects, OutputType outputType);
    }
}