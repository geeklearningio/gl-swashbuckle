


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
    using global::Swashbuckle.Swagger.Model;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Mvc;
    using global::Swashbuckle.Swagger.Application;
    using Newtonsoft.Json;

    public class Program
    {
        public static void Main(string[] args)
        {
            var options = CommandLineOptions.Parse(args);

            var project = Create(options.TargetProject ?? Directory.GetCurrentDirectory(), options.Framework);

            var outputPath = project.GetOutputPaths(options.Configuration, /* buildBasePath: */ null, null);

            var isExecutable = project.ProjectFile.GetCompilerOptions(project.TargetFramework, options.Configuration).EmitEntryPoint
                            ?? project.ProjectFile.GetCompilerOptions(null, options.Configuration).EmitEntryPoint.GetValueOrDefault();

            var assemblyPath = isExecutable && (project.IsPortable || options.Framework.IsDesktop())
                ? outputPath.RuntimeFiles.Executable
                : outputPath.RuntimeFiles.Assembly;

            System.Reflection.Assembly assembly;

            //assembly = System.Reflection.Assembly.LoadFile(assemblyPath);
#if NET46
            assembly = System.Reflection.Assembly.LoadFile(assemblyPath);
#else
            assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
#endif

            var startup = assembly.GetTypes().Where(x => x.Name == "Startup").FirstOrDefault();

            var genericTestEnvironment = typeof(GeekLearning.Test.Integration.Environment.TestEnvironment<,>);
            var testEnvironmentType = genericTestEnvironment.MakeGenericType(startup, typeof(GeekLearning.Test.Integration.Environment.DefaultStartupConfigurationService));

            var testEnvironment = (GeekLearning.Test.Integration.Environment.ITestEnvironment)Activator.CreateInstance(testEnvironmentType);

            var swaggerProvider = testEnvironment.ServiceProvider.GetRequiredService<ISwaggerProvider>();
            var mvcJsonOptions = testEnvironment.ServiceProvider.GetRequiredService<IOptions<MvcJsonOptions>>();

            var swaggerSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new SwaggerContractResolver(mvcJsonOptions.Value.SerializerSettings)
            };

            var document = swaggerProvider.GetSwagger(options.ApiVersion);

            using (var fs = new FileStream(options.OutputPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var writer = new StreamWriter(fs))
                {
                    swaggerSerializer.Serialize(writer, document);
                }
            }
        }

        public static ProjectContext Create(string filePath,
          NuGetFramework framework)
        {
            return SelectCompatibleFramework(
                 framework,
                 ProjectContext.CreateContextForEachFramework(filePath,
                     runtimeIdentifiers: RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers()));
        }

        private static ProjectContext SelectCompatibleFramework(NuGetFramework target, IEnumerable<ProjectContext> contexts)
        {
            return NuGetFrameworkUtility.GetNearest(contexts, target ?? FrameworkConstants.CommonFrameworks.NetCoreApp10, f => f.TargetFramework)
                   ?? contexts.First();
        }
    }
}
