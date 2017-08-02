


namespace GeekLearning.DotNet.Swashbuckle
{
    using Microsoft.DotNet.Cli.Utils;
    using Microsoft.DotNet.InternalAbstractions;
    using Microsoft.DotNet.ProjectModel;
    using Microsoft.Extensions.DependencyInjection;
    using NuGet.Frameworks;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Mvc;
    using global::Swashbuckle.AspNetCore.Swagger;
    using global::Swashbuckle.AspNetCore.SwaggerGen;
    using Newtonsoft.Json;
    using Microsoft.Extensions.PlatformAbstractions;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.AspNetCore.Hosting;
    using System.Reflection;
    using Microsoft.Extensions.DependencyModel;
    using System.Runtime.Loader;
    using HandlebarsDotNet;

    public class Program
    {
        private static string folderPath;

        public static void Main(string[] args)
        {
            Console.WriteLine(string.Join(" ", args));

            var options = CommandLineOptions.Parse(args);

            var project = Create(options.TargetProject ?? Directory.GetCurrentDirectory(), options.Framework);

            var configuration = options.Configuration ?? "Debug";


            var outputPath = project.GetOutputPaths(configuration, /* buildBasePath: */ null, null);

            var isExecutable = project.ProjectFile.GetCompilerOptions(project.TargetFramework, configuration).EmitEntryPoint
                            ?? project.ProjectFile.GetCompilerOptions(null, configuration).EmitEntryPoint.GetValueOrDefault();


            Console.WriteLine(project.TargetFramework.DotNetFrameworkName);
            Console.WriteLine(PlatformServices.Default.Application.RuntimeFramework.FullName);

            var assemblyPath = isExecutable && (project.IsPortable || options.Framework.IsDesktop())
                ? outputPath.RuntimeFiles.Executable
                : outputPath.RuntimeFiles.Assembly;

            Console.WriteLine(project.PackagesDirectory);

            var tempProjectPath = Path.Combine(outputPath.IntermediateOutputDirectoryPath, "SwaggerGen");

            if (!Directory.Exists(tempProjectPath))
            {
                Directory.CreateDirectory(tempProjectPath);
            }



            var tempProjectPathUri = new Uri(tempProjectPath);

            var projectDependency = tempProjectPathUri.MakeRelativeUri(new Uri(project.ProjectDirectory)).ToString();

            var generationContext = new
            {
                AssemblyPath = assemblyPath,
                ContentRoot = project.ProjectDirectory,
                OutputPath = Path.IsPathRooted(options.OutputPath) ? options.OutputPath : Path.Combine(project.ProjectDirectory, options.OutputPath),
                ApiVersion = options.ApiVersion,
                ProjectDependency = new DirectoryInfo(project.ProjectDirectory).Name,
                IsNetCoreApp = project.TargetFramework.DotNetFrameworkName == ".NETCoreApp,Version=v1.0"
            };

            var ass = Assembly.GetEntryAssembly();

            foreach (var filePath in ass.GetManifestResourceNames().Where(x => x.EndsWith(".hbs")))
            {
                using (var reader = new StreamReader(ass.GetManifestResourceStream(filePath)))
                {
                    var template = Handlebars.Compile(reader.ReadToEnd());
                    File.WriteAllText(Path.Combine(tempProjectPath, Path.GetFileNameWithoutExtension(filePath.Replace("GeekLearning.DotNet.Swashbuckle.ProjectTemplate.", ""))), template(generationContext));
                }
            }

            var restore = Command.CreateDotNet("restore", new string[] { });

            restore.WorkingDirectory(tempProjectPath);
            var result = restore.Execute();
            if (result.ExitCode == 0)
            {
                var run = Command.CreateDotNet("run", new string[] { }, configuration: configuration);
                run.WorkingDirectory(tempProjectPath);
                result = run.Execute();

            }

            Directory.Delete(tempProjectPath, true);
        }




        public static ProjectContext Create(string filePath,
          NuGetFramework framework)
        {
            return SelectCompatibleFramework(
                 framework,
                 ProjectContext.CreateContextForEachFramework(filePath,
                     runtimeIdentifiers: Microsoft.DotNet.Cli.Utils.RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers()));
        }

        private static ProjectContext SelectCompatibleFramework(NuGetFramework target, IEnumerable<ProjectContext> contexts)
        {
            return NuGetFrameworkUtility.GetNearest(contexts, target ?? FrameworkConstants.CommonFrameworks.NetCoreApp10, f => f.TargetFramework)
                   ?? contexts.First();
        }
    }
}
