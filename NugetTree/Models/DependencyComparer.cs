using System.Collections.Generic;

namespace NugetTree.Models
{
    internal class DependencyComparer : IEqualityComparer<Dependency>
    {
        public bool Equals(Dependency x, Dependency y)
        {
            return x?.Name == y?.Name && x?.Version == y?.Version;
        }

        public int GetHashCode(Dependency obj)
        {
            return $"{obj?.Name}_{obj?.Version}".GetHashCode();
        }
    }
}