namespace GeekLearning.SwashbuckleExtensions
{
    using Swashbuckle.Swagger.Model;
    using Swashbuckle.SwaggerGen.Generator;
    using System.Collections.Generic;

    public class AssignExamples : IOperationFilter
    {
        private ExamplesBuilder builder;

        public AssignExamples(ExamplesBuilder builder)
        {
            this.builder = builder;
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            Response successResponse;

            if (operation.Responses.TryGetValue("200", out successResponse) || operation.Responses.TryGetValue("204", out successResponse))
            {
            }

            Dictionary<string, Dictionary<string, object>> actionSamples;

            if (this.builder.Examples.TryGetValue(context.ApiDescription.ActionDescriptor.DisplayName, out actionSamples))
            {
                foreach (var statusSamples in actionSamples)
                {
                    Response statusResponse;
                    if (!operation.Responses.TryGetValue(statusSamples.Key, out statusResponse))
                    {
                        statusResponse = new Response();
                        operation.Responses[statusSamples.Key] = statusResponse;
                        if (successResponse != null)
                        {
                            statusResponse.Schema = successResponse.Schema;
                        }
                    }

                    statusResponse.Examples = statusSamples.Value;
                }
            }
        }
    }
}
