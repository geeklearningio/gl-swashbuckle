namespace GeekLearning.Domain.SwahbuckleExtensions
{
    using Swashbuckle.AspNetCore.SwaggerGen;
    using Swashbuckle.AspNetCore.Swagger;
    using GeekLearning.Domain.AspnetCore;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using System.Threading.Tasks;

    public class MaybeResultDocumentFilter : IOperationFilter
    {

        public void Apply(Operation operation, OperationFilterContext context)
        {
            switch (context.ApiDescription.ActionDescriptor)
            {
                case ControllerActionDescriptor action:
                    if (action.MethodInfo.ReturnType.IsConstructedGenericType)
                    {
                        var returnType = action.MethodInfo.ReturnType;
                        var genericTypeDefinition = returnType.GetGenericTypeDefinition();

                        if (genericTypeDefinition == typeof(Task<>))
                        {
                            returnType = returnType.GenericTypeArguments[0];
                            if (returnType.IsConstructedGenericType)
                            {
                                genericTypeDefinition = returnType.GetGenericTypeDefinition();
                            }
                        }

                        if (genericTypeDefinition == typeof(MaybeResult<>))
                        {
                            var responseType = typeof(Response<>).MakeGenericType(returnType.GenericTypeArguments[0]);

                            operation.Responses.TryGetValue("200", out Response okResponse);

                            if (okResponse == null)
                            {
                                okResponse = new Response();
                            }

                            okResponse.Schema = context.SchemaRegistry.GetOrRegister(responseType);

                            operation.Responses["200"] = okResponse;
                            if (!operation.Produces.Contains("application/json"))
                            {
                                operation.Produces.Add("application/json");
                            }
                        }
                    }

                    break;
                default:
                    break;
            }
        }
    }
}
