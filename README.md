# NugetTree
Project which reads nuget dependencies and has options for output

## To Run
`dotnet NugetTree.dll "C:\path\to\solutionfile.sln" [output type(s)]`

Available outputs:
```
-c   "Conflicts" => lists version mismatches by dependency name
-f   "Flat" => lists all dependencies by project in a flattened view
-t   "Tree" => lists full nuget dependency tree
-fv  "Framework Version" => lists projects by targeted framework
-nr  "Non-Recursive" => lists only top level dependencies, by project
```

*Example*
`dotnet NugetTree.dll "NugetTree.sln" -c`

#### Multiple outputs can be passed at the same time and will be displayed in the same order they appear in the argument list
*Example*
`dotnet NugetTree.dll "NugetTree.sln" -t -c`
