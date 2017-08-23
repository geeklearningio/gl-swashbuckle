[![NuGet Version](http://img.shields.io/nuget/v/GeekLearning.SwashbuckleExtensions.svg?style=flat-square&label=NuGet:%20ASP.NET%20Core%20Extensions)](https://www.nuget.org/packages/GeekLearning.SwashbuckleExtensions/)
[![NuGet Version](http://img.shields.io/nuget/v/GeekLearning.DotNet.Swashbuckle.svg?style=flat-square&label=NuGet:%20.NET%20Core%20CLI%20Tool)](https://www.nuget.org/packages/GeekLearning.DotNet.Swashbuckle/)
[![Build Status](https://geeklearning.visualstudio.com/_apis/public/build/definitions/f841b266-7595-4d01-9ee1-4864cf65aa73/30/badge)](#)

# Geek Learning Swashbuckle Extensions

This project provides extensions and tools for Swashbuckle in an ASP.NET Core application.

## GeekLearning.SwashbuckleExtensions

### Operation Filters

* **AssignExamples**: add examples in your generated Swagger file
* **AssignSecurityRequirements**: add security requirements based on your ASP.NET security attributes
* **DropTextPlainConsumes**: delete the `text/plain` content type from the `consumes` operation list
* **DropTextPlainProduces**: delete the `text/plain` content type from the `produces` operation list
* **FormFileOperationFilter**: detect `IFormFile` parameters to properly set the Swagger parameter type
* **SwaggerFormOperationFilter**: read `SwaggerFormParameter` attributes allowing you to specify additional elements not necesseraly present in the ApiDescription.

### Document Filters

* **SchemesDocumentFilter**: apply selected schemes to the Swagger document

## GeekLearning.DotNet.Swashbuckle
  
### Name

`dotnet-swashbuckle` - Generate a Swagger JSON file for an offline ASP.NET Core project.

### Synopsis

`dotnet swashbuckle [-p|--project] [-so|--swagger-output] [-f|--framework] [-c|--configuration] [-sv|--swagger-api-version] [-h|--help] [--version]`

### Description

The `dotnet swashbuckle` command allows you to extract a Swagger file from an ASP.NET Core application without having to run it.

It can be useful if you want to build and deploy client librairies during your CI builds.

### Options

`-p|--project <project>`

The project to target (defaults to the project in the current directory). Can be a path to a csproj file or a project directory. The targeted project must be an ASP.NET Core application, with a `Startup` class and with the [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) services registered.

`-f|--framework <framework>`

Compiles for a specific framework. The framework must be defined in the project file.

`-c|--configuration <configuration>`

Defines the build configuration. If omitted, the build configuration defaults to `Debug`. Use `Release` build a Release configuration.

`-so|--swagger-output <swagger_output_file>`

File in which to write Swagger schema.

`-sv|--swagger-api-version <api_version>`

The API version you want to generate Swagger definition for.

`--version`

Show version information.

`-h|--help`

Show help information.

### Examples

Write a `swagger.json` file in the current directory, which is an ASP.NET Core application:

`dotnet swashbuckle`

Write a `my-swagger.json` file in the current directory, which is an ASP.NET Core application, that contains the `v1` API definition:

`dotnet swashbuckle -sv v1 -so my-swagger.json`

Write a `result.json` file in the `src/MyWebApp` directory, which is an ASP.NET Core application, compiled for the `net462` framework in `Release` configuration, that contains the `v1` API definition:

`dotnet swashbuckle -p src/MyWebApp -f net462 -c Release -so result.json -sv v1`
