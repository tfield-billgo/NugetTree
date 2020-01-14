using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NugetTree.Models
{
    public class Dependency : IHasDependencies
    {
        private static readonly Regex THREE_PART_VERSION = new Regex(@"^\d+\.\d+\.\d+$", RegexOptions.Compiled);

        private List<Dependency> _dependencies;
        private string _version;

        public IEnumerable<Dependency> Dependencies
        {
            get { return _dependencies ?? (_dependencies = new List<Dependency>()); }
            set { _dependencies = value?.ToList(); }
        }

        public bool FoundInSources { get; set; }

        public string Framework { get; set; }

        public string Name { get; set; }

        public string Project { get; set; }

        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                var val = value ?? string.Empty;
                if (THREE_PART_VERSION.IsMatch(val))
                {
                    val += ".0";
                }
                _version = val;
            }
        }

        public string VersionLimited { get; set; }
    }
}