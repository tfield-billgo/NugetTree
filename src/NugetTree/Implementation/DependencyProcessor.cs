using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NugetTree.Core;
using NugetTree.Models;

namespace NugetTree.Implementation
{
    public sealed class DependencyProcessor : IDependencyProcessor
    {
        private static readonly Regex PROJECT_PATH_REGEX = new Regex(@"""([^""]+\.csproj)""", RegexOptions.Compiled);

        private readonly IProjectReader _projectReader;
        private readonly IProjectWriter _projectWriter;

        public DependencyProcessor(IProjectReader projectReader, IProjectWriter projectWriter)
        {
            _projectReader = projectReader ?? throw new ArgumentNullException(nameof(projectReader));
            _projectWriter = projectWriter ?? throw new ArgumentNullException(nameof(projectWriter));
        }

        public async Task<IEnumerable<ProjectDependencyInfo>> ProcessSolution(string solutionFileName, params OutputType[] outputTypes)
        {
            if (string.IsNullOrEmpty(solutionFileName))
                throw new ArgumentNullException(nameof(solutionFileName));

            if (!File.Exists(solutionFileName))
                throw new ArgumentException($"Solution does not exist: {solutionFileName}", nameof(solutionFileName));

            var solutionContent = File.ReadAllText(solutionFileName);
            var matches = PROJECT_PATH_REGEX.Matches(solutionContent);

            var solutionName = solutionFileName.Substring(solutionFileName.LastIndexOf("\\") + 1);
            var projects = new List<ProjectDependencyInfo>();

            foreach (Match match in matches)
            {
                var group = match.Groups[1];
                var projectFilename = solutionFileName.Replace(solutionName, group.Value);

                var project = await _projectReader.ReadProjectFile(projectFilename);
                projects.Add(project);
            }

            projects = projects.OrderBy(p => p.ProjectName).ToList();

            foreach (var outputType in outputTypes)
            {
                _projectWriter.WriteProjects(projects, outputType);
            }

            return projects;
        }
    }
}