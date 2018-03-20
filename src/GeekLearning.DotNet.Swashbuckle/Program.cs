namespace GeekLearning.DotNet.Swashbuckle
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using HandlebarsDotNet;
    using Microsoft.DotNet.Cli.Utils;

    public class Program
    {
        private const string NetCoreAppFramework = "netcoreapp";
        private const string TargetFrameworkProperty = "TargetFramework";
        private const string TargetFrameworksProperty = "TargetFrameworks";
        private const string IntermediateOutputPathProperty = "IntermediateOutputPath";
        private const string AssemblyNameProperty = "AssemblyName";
        private const string ReferencesProperty = "References";

        public static void Main(string[] args)
        {
            var options = CommandLineOptions.Parse(args);
            if (options.IsHelp)
            {
                return;
            }

            var projectFile = FindMsBuildProject(Directory.GetCurrentDirectory(), options.TargetProject);
            var projectDirectory = Path.GetDirectoryName(projectFile);
            var (properties, references) = ReadProperties(projectFile, options.Configuration, options.Framework?.Framework);

            var tempProjectPath = Path.Combine(projectDirectory, properties[IntermediateOutputPathProperty], "SwaggerGen");
            if (!Directory.Exists(tempProjectPath))
            {
                Directory.CreateDirectory(tempProjectPath);
            }

            var tempProjectPathUri = new Uri(tempProjectPath);

            var generationContext = new
            {
                ProjectPath = projectFile,
                TargetFramework = properties[TargetFrameworkProperty],
                AssemblyName = properties[AssemblyNameProperty],
                ContentRoot = projectDirectory,
                ApiVersion = options.ApiVersion,
                OutputPath = Path.IsPathRooted(options.OutputPath) ? options.OutputPath : Path.Combine(projectDirectory, options.OutputPath),
                References = references.ToDictionary(x => x.Key.Replace(".", ""), y => y.Value)
            };

            var assembly = typeof(Program).GetTypeInfo().Assembly;
            foreach (var filePath in assembly.GetManifestResourceNames().Where(x => x.EndsWith(".hbs")))
            {
                using (var reader = new StreamReader(assembly.GetManifestResourceStream(filePath)))
                {
                    var template = Handlebars.Compile(reader.ReadToEnd());
                    File.WriteAllText(
                        Path.Combine(
                            tempProjectPath,
                            Path.GetFileNameWithoutExtension(
                                filePath
                                    .Replace("GeekLearning.DotNet.Swashbuckle.ProjectTemplate.", "")
                                    .Replace(".cst", ".cs"))),
                        template(generationContext));
                }

            }

            var restore = Command.CreateDotNet("restore", new string[] { });

            restore.WorkingDirectory(tempProjectPath);
            var result = restore.Execute();
            if (result.ExitCode == 0)
            {
                var run = Command.CreateDotNet("run", new string[] { }, configuration: options.Configuration);
                if (!string.IsNullOrEmpty(options.AspnetcoreEnvironment))
                {
                    run.EnvironmentVariable("ASPNETCORE_ENVIRONMENT", options.AspnetcoreEnvironment);
                }

                run.WorkingDirectory(tempProjectPath);
                result = run.Execute();
            }

            Directory.Delete(tempProjectPath, true);
        }

        private static string FindMsBuildProject(string searchBase, string project)
        {
            if (string.IsNullOrEmpty(searchBase))
            {
                throw new ArgumentException("Value cannot be null or an empty string.", nameof(searchBase));
            }

            var projectPath = project ?? searchBase;
            if (!Path.IsPathRooted(projectPath))
            {
                projectPath = Path.Combine(searchBase, projectPath);
            }

            if (Directory.Exists(projectPath))
            {
                var projects = Directory.EnumerateFileSystemEntries(projectPath, "*.*proj", SearchOption.TopDirectoryOnly)
                    .Where(f => !".xproj".Equals(Path.GetExtension(f), StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (projects.Count > 1)
                {
                    throw new FileNotFoundException($"Multiple MSBuild project files found in '{projectPath}'. Specify which to use with the --project option.");
                }

                if (projects.Count == 0)
                {
                    throw new FileNotFoundException($"Could not find a MSBuild project file in '{projectPath}'. Specify which project to use with the --project option.");
                }

                return projects[0];
            }

            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException($"The project file '{projectPath}' does not exist.");
            }

            return projectPath;
        }

        private static (Dictionary<string, string> Properties, Dictionary<string, string> References) ReadProperties(string projectFile, string configuration, string framework)
        {
            var targetFileName = Path.GetFileName(projectFile) + ".dotnet-names.targets";
            var projectExtPath = Path.Combine(Path.GetDirectoryName(projectFile), "obj");
            var targetFile = Path.Combine(projectExtPath, targetFileName);

            File.WriteAllText(
                targetFile,
                $@"<Project>
                      <Target Name=""_GetDotNetNames"" DependsOnTargets=""ResolvePackageDependenciesDesignTime"">
                         <ItemGroup>
                            <_DotNetNamesOutput Include=""{AssemblyNameProperty}: $({AssemblyNameProperty})"" />
                            <_DotNetNamesOutput Include=""{TargetFrameworkProperty}: $({TargetFrameworkProperty})"" />
                            <_DotNetNamesOutput Include=""{TargetFrameworksProperty}: $({TargetFrameworksProperty})"" />
                            <_DotNetNamesOutput Include=""{IntermediateOutputPathProperty}: $({IntermediateOutputPathProperty})"" />
                         </ItemGroup>
                         <WriteLinesToFile File=""$(_DotNetNamesFile)"" Lines=""@(_DotNetNamesOutput)"" Overwrite=""true"" />
                         <WriteLinesToFile File=""$(_DotNetReferencesFile)"" Lines=""@(_DependenciesDesignTime)"" Overwrite=""true"" />
                      </Target>
                  </Project>");

            var additionnalParameters = string.Empty;
            if (!string.IsNullOrEmpty(framework))
            {
                additionnalParameters = $"/p:{TargetFrameworkProperty}={framework}";
            }

            var dotNetNamesFileName = Path.GetTempFileName();
            var dotNetReferencesFileName = Path.GetTempFileName();

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"msbuild \"{projectFile}\" /p:Configuration={configuration} /t:_GetDotNetNames /nologo \"/p:_DotNetNamesFile={dotNetNamesFileName}\" \"/p:_DotNetReferencesFile={dotNetReferencesFileName}\" {additionnalParameters}"
            };

            var process = Process.Start(psi);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception("Invoking MSBuild target failed");
            }

            var lines = File.ReadAllLines(dotNetNamesFileName);
            File.Delete(dotNetNamesFileName);

            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                var index = line.IndexOf(':');
                if (index <= 0)
                {
                    continue;
                }

                var name = line.Substring(0, index)?.Trim();
                var value = line.Substring(index + 1)?.Trim();
                properties.Add(name, value);
            }

            var referencesLines = File.ReadAllLines(dotNetReferencesFileName);
            var references = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in referencesLines)
            {
                var parts = line.Split('/');
                if (parts.Length == 3)
                {
                    references.Add(parts[1], parts[2]);
                }
            }

            return (properties, references);
        }
    }
}
