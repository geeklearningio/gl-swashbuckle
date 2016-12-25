﻿/// <summary>
/// Thanks to RehanSaeed - https://github.com/domaindrivendev/Ahoy/issues/193
/// </summary>


namespace GeekLearning.SwashbuckleExtensions
{
    using Microsoft.AspNetCore.Http;
    using Swashbuckle.Swagger.Model;
    using Swashbuckle.SwaggerGen.Generator;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class FormFileOperationFilter : IOperationFilter
    {
        private const string FormDataMimeType = "multipart/form-data";
        private static readonly string[] FormFilePropertyNames =
            typeof(IFormFile).GetTypeInfo().DeclaredProperties.Select(x => x.Name).ToArray();

        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ParameterDescriptions.Any(x => x.ModelMetadata.ContainerType == typeof(IFormFile)))
            {
                var formFileParameters = operation
                    .Parameters
                    .OfType<NonBodyParameter>()
                    .Where(x => FormFilePropertyNames.Contains(x.Name))
                    .ToArray();
                var index = operation.Parameters.IndexOf(formFileParameters.First());
                foreach (var formFileParameter in formFileParameters)
                {
                    operation.Parameters.Remove(formFileParameter);
                }

                var formFileParameterName = context
                    .ApiDescription
                    .ActionDescriptor
                    .Parameters
                    .Where(x => x.ParameterType == typeof(IFormFile))
                    .Select(x => x.Name)
                    .First();
                var parameter = new NonBodyParameter()
                {
                    Name = formFileParameterName,
                    In = "formData",
                    Description = "The file to upload.",
                    Required = true,
                    Type = "file"
                };
                operation.Parameters.Insert(index, parameter);

                if (!operation.Consumes.Contains(FormDataMimeType))
                {
                    operation.Consumes.Add(FormDataMimeType);
                }
            }
        }
    }
}
