namespace GeekLearning.DotNet.Swashbuckle
{
    using HandlebarsDotNet;
    using Microsoft.DotNet.Cli.Utils;
    using Microsoft.Extensions.PlatformAbstractions;
    using NuGet.Frameworks;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class Program
    {
        private const string netCoreAppFramework = "netcoreapp";
        private const string targetFrameworkProperty = "TargetFramework";
        private const string targetFrameworksProperty = "TargetFrameworks";
        private const string intermediateOutputPathProperty = "IntermediateOutputPath";
        private const string assemblyNameProperty = "AssemblyName";

        public static void Main(string[] args)
        {
            var options = CommandLineOptions.Parse(args);

            var projectFile = FindMsBuildProject(Directory.GetCurrentDirectory(), options.TargetProject);
            var projectDirectory = Path.GetDirectoryName(projectFile);
            var properties = ReadProperties(projectFile, options.Configuration, options.Framework?.Framework);

            var hasNetCoreAppFramework = false;
            if (properties.TryGetValue(targetFrameworkProperty, out var framework)
                && !string.IsNullOrEmpty(framework)
                && framework.StartsWith(netCoreAppFramework))
            {
                hasNetCoreAppFramework = true;
            }

            if (properties.TryGetValue(targetFrameworksProperty, out var tfms)
                && !string.IsNullOrEmpty(tfms))
            {
                foreach (var tfm in tfms.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (tfm.StartsWith(netCoreAppFramework))
                    {
                        hasNetCoreAppFramework = true;
                        break;
                    }
                }
            }

            var tempProjectPath = Path.Combine(projectDirectory, properties[intermediateOutputPathProperty], "SwaggerGen");
            if (!Directory.Exists(tempProjectPath))
            {
                Directory.CreateDirectory(tempProjectPath);
            }

            var tempProjectPathUri = new Uri(tempProjectPath);

            var generationContext = new
            {
                ProjectPath = projectFile,
                IsNetCoreApp = hasNetCoreAppFramework && (options.Framework == null || options.Framework.Framework.StartsWith(netCoreAppFramework)),
                AssemblyName = properties[assemblyNameProperty],
                ContentRoot = projectDirectory,
                ApiVersion = options.ApiVersion,
                OutputPath = Path.IsPathRooted(options.OutputPath) ? options.OutputPath : Path.Combine(projectDirectory, options.OutputPath),
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

        private static Dictionary<string, string> ReadProperties(string projectFile, string configuration, string framework)
        {
            var targetFileName = Path.GetFileName(projectFile) + ".dotnet-names.targets";
            var projectExtPath = Path.Combine(Path.GetDirectoryName(projectFile), "obj");
            var targetFile = Path.Combine(projectExtPath, targetFileName);

            File.WriteAllText(targetFile,
$@"<Project>
      <Target Name=""_GetDotNetNames"">
         <ItemGroup>
            <_DotNetNamesOutput Include=""{assemblyNameProperty}: $({assemblyNameProperty})"" />
            <_DotNetNamesOutput Include=""{targetFrameworkProperty}: $({targetFrameworkProperty})"" />
            <_DotNetNamesOutput Include=""{targetFrameworksProperty}: $({targetFrameworksProperty})"" />
            <_DotNetNamesOutput Include=""{intermediateOutputPathProperty}: $({intermediateOutputPathProperty})"" />
         </ItemGroup>
         <WriteLinesToFile File=""$(_DotNetNamesFile)"" Lines=""@(_DotNetNamesOutput)"" Overwrite=""true"" />
      </Target>
  </Project>");

            var additionnalParameters = string.Empty;
            if (!string.IsNullOrEmpty(framework))
            {
                additionnalParameters = $"/p:{targetFrameworkProperty}={framework}";
            }

            var tmpFile = Path.GetTempFileName();
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"msbuild \"{projectFile}\" /p:Configuration={configuration} /t:_GetDotNetNames /nologo \"/p:_DotNetNamesFile={tmpFile}\" {additionnalParameters}"
            };

            var process = Process.Start(psi);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception("Invoking MSBuild target failed");
            }

            var lines = File.ReadAllLines(tmpFile);
            File.Delete(tmpFile);

            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                var idx = line.IndexOf(':');
                if (idx <= 0) continue;
                var name = line.Substring(0, idx)?.Trim();
                var value = line.Substring(idx + 1)?.Trim();
                properties.Add(name, value);
            }

            return properties;
        }
    }
}
