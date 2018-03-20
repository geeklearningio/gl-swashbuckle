namespace GeekLearning.DotNet.Swashbuckle
{
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.DotNet.Cli.Utils;
    using Microsoft.Extensions.CommandLineUtils;
    using NuGet.Frameworks;

    public class CommandLineOptions
    {
        private static readonly Assembly ThisAssembly = typeof(CommandLineOptions).GetTypeInfo().Assembly;
        private static readonly string AssemblyVersion = ThisAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? ThisAssembly.GetName().Version.ToString();

        public string Configuration { get; set; }

        public string TargetProject { get; set; }

        public NuGetFramework Framework { get; set; }

        public string OutputPath { get; set; }

        public IList<string> RemainingArguments { get; set; }

        public bool IsHelp { get; set; }

        public string ApiVersion { get; set; }

        public string AspnetcoreEnvironment { get; set; }

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
                "The project to target (defaults to the project in the current directory). Can be a path to a csproj file or a project directory. The targeted project must be an ASP.NET Core application, with a `Startup` class and with the Swashbuckle.AspNetCore services registered.",
                CommandOptionType.SingleValue);

            var framework = app.Option(
                "-f|--framework <framework>",
                $"Compiles for a specific framework. The framework must be defined in the project file.",
                CommandOptionType.SingleValue);

            var configuration = app.Option(
                "-c|--configuration <configuration>",
                $"Defines the build configuration. If omitted, the build configuration defaults to `{Constants.DefaultConfiguration}`. Use `Release` build a Release configuration.",
                CommandOptionType.SingleValue);

            var output = app.Option(
                "-so|--swagger-output <swagger_output_file>",
                "File in which to write Swagger schema.",
                CommandOptionType.SingleValue);

            var apiVersion = app.Option(
                "-sv|--swagger-api-version <api_version>",
                "The API version you want to generate Swagger definition for.",
                CommandOptionType.SingleValue);

            var aspnetcoreEnvironment = app.Option(
                "-av|--aspnetcore-environement <aspnetcore_environement>",
                "Optional override for ASPNETCORE_ENVIRONMENT variable",
                CommandOptionType.SingleValue
                );

            app.VersionOption("--version", () => AssemblyVersion);

            app.HelpOption("-h|--help");

            app.OnExecute(() =>
            {
                options.TargetProject = project.Value();
                options.Configuration = configuration.Value() ?? Constants.DefaultConfiguration;
                options.Framework = framework.HasValue() ? NuGetFramework.Parse(framework.Value()) : null;
                options.OutputPath = output.Value() ?? "swagger.json";
                options.ApiVersion = apiVersion.Value() ?? "v1";
                options.RemainingArguments = app.RemainingArguments;
                options.AspnetcoreEnvironment = aspnetcoreEnvironment.Value();
                return 0;
            });
        }
    }
}
