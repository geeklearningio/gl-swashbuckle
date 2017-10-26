namespace GeekLearning.SwashbuckleExtensions
{
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Bring support for additional Form parameters without any Model Binder interference. Can be used to tweak swagger UI 
    /// or describe form-data based call with Model Binder deactivated
    /// Thanks to jonnybi - https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/193
    /// </summary>
    public class SwaggerFormOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {

            var requestAttributes = context.ApiDescription.ActionAttributes().Concat(context.ApiDescription.ControllerAttributes()).OfType<SwaggerFormParameterAttribute>();

            if (requestAttributes.Any())
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();

                var parametersToClean = operation.Parameters.OfType<SwaggerFormParameter>().ToArray();

                foreach (var parameter in parametersToClean)
                {
                    operation.Parameters.Remove(parameter);
                }

                if (operation.Consumes.Count == 0)
                    operation.Consumes.Add("multipart/form-data");
            }

            foreach (var attr in requestAttributes)
            {
                operation.Parameters.Add(
                    new SwaggerFormParameter
                    {
                        Name = attr.Name,
                        Description = attr.Description,
                        In = "formData",
                        Required = attr.IsRequired,
                        Type = attr.Type,
                    });

            }
        }


        private class SwaggerFormParameter : NonBodyParameter
        {

        }
    }
}
