using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NugetTree.Models;

namespace NugetTree
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var targetSolution = default(string);
            var outputs = new List<OutputType>();

            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    switch (arg)
                    {
                        case "-c":
                            outputs.Add(OutputType.Conflicts);
                            break;

                        case "-f":
                            outputs.Add(OutputType.Flat);
                            break;

                        case "-t":
                            outputs.Add(OutputType.Tree);
                            break;

                        case "-nr":
                            outputs.Add(OutputType.TopLevel);
                            break;

                        case "-fv":
                            outputs.Add(OutputType.Frameworks);
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    targetSolution = arg;
                }
            }

            if (String.IsNullOrEmpty(targetSolution))
            {
                Console.WriteLine("You must provide a path to a solution file");
                return;
            }

            if (!File.Exists(targetSolution))
            {
                Console.WriteLine($"Solution not found {targetSolution}");
                return;
            }

            if (!outputs.Any())
            {
                Console.WriteLine("You must provide at least one output");
                return;
            }

            var processor = await DependencyProcessorFactory.CreateDependencyProcessorAsync(NuGet.Common.LogLevel.Warning);

            await processor.ProcessSolution(targetSolution, outputs.ToArray());
        }
    }
}