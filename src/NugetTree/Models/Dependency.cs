using System.Collections.Generic;
using System.Linq;

namespace NugetTree.Models
{
    public class Dependency : IHasDependencies
    {
        private List<Dependency> _dependencies;

        public IEnumerable<Dependency> Dependencies
        {
            get { return _dependencies ?? (_dependencies = new List<Dependency>()); }
            set { _dependencies = value?.ToList(); }
        }

        public bool FoundInSources { get; set; }

        public string Framework { get; set; }

        public string Name { get; set; }

        public string Project { get; set; }

        public string Version { get; set; }

        public string VersionLimited { get; set; }
    }
}