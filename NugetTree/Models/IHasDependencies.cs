using System.Collections.Generic;

namespace NugetTree.Models
{
    public interface IHasDependencies
    {
        IEnumerable<Dependency> Dependencies { get; }

        public IEnumerable<Dependency> RecurseDependencies()
        {
            foreach (var dependency in Dependencies)
            {
                yield return dependency;
                foreach (var item in ((IHasDependencies)dependency).RecurseDependencies())
                {
                    yield return item;
                }
            }
        }
    }
}