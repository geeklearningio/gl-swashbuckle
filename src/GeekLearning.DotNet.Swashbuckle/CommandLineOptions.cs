namespace GeekLearning.DotNet.Swashbuckle
{
    using Microsoft.DotNet.Cli.Utils;
    using Microsoft.Extensions.CommandLineUtils;
    using NuGet.Frameworks;
    using System.Collections.Generic;
    using System.Reflection;

    public class CommandLineOptions
    {
        private static readonly Assembly thisAssembly = typeof(CommandLineOptions).GetTypeInfo().Assembly;
        private static readonly string assemblyVersion = thisAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? thisAssembly.GetName().Version.ToString();

        public string Configuration { get; set; }

        public string TargetProject { get; set; }

        public NuGetFramework Framework { get; set; }

        public string OutputPath { get; set; }

        public IList<string> RemainingArguments { get; set; }

        public bool IsHelp { get; set; }

        public string ApiVersion { get; set; }

        public static CommandLineOptions Parse(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "dotnet swashbuckle",
                FullName = "Swashbuckle .NET Core CLI Commands"
            };

            var options = new CommandLineOptions();

            Configure(app, options);

            app.Execute(args);
            options.IsHelp = app.IsShowingInformation;

            return options;
        }

        private static void Configure(CommandLineApplication app, CommandLineOptions options)
        {
            var project = app.Option(
                "-p|--project <project>",
                "The project to target (defaults to the project in the current directory). Can be a path to a project.json or a project directory.",
                CommandOptionType.SingleValue);

            var configuration = app.Option(
                "--configuration <configuration>",
                $"Configuration under which to load (defaults to {Constants.DefaultConfiguration})",
                CommandOptionType.SingleValue);

            var framework = app.Option(
                "-f|--framework <framework>",
                $"Target framework to load from the startup project (defaults to the framework most compatible with {FrameworkConstants.CommonFrameworks.NetCoreApp11}).",
                CommandOptionType.SingleValue);

            var output = app.Option(
                "-so|--swagger-output <swagger_output_file>",
                "File in which to write swagger schema",
                CommandOptionType.SingleValue);

            var apiVersion = app.Option(
                "-v|--api-version <api_version>",
                "The api version you want to generate swagger definition for.",
                CommandOptionType.SingleValue);

            app.VersionOption("--version", () => assemblyVersion);

            app.OnExecute(() =>
            {
                options.TargetProject = project.Value();
                options.Configuration = configuration.Value() ?? Constants.DefaultConfiguration;
                options.Framework = framework.HasValue() ? NuGetFramework.Parse(framework.Value()) : null;
                options.OutputPath = output.Value();
                options.ApiVersion = apiVersion.Value();
                options.RemainingArguments = app.RemainingArguments;
                return 0;
            });
        }
    }
}
