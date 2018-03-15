namespace GeekLearning.SwashbuckleExtensions
{
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class AssignExamples : IOperationFilter
    {
        private ExamplesBuilder builder;

        public AssignExamples(ExamplesBuilder builder)
        {
            this.builder = builder;
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            var isSuccessResponse = operation.Responses.TryGetValue("200", out var successResponse)
                || operation.Responses.TryGetValue("204", out successResponse);

            if (this.builder.Examples.TryGetValue(context.ApiDescription.ActionDescriptor.DisplayName, out var actionSamples))
            {
                foreach (var statusSamples in actionSamples)
                {
                    if (!operation.Responses.TryGetValue(statusSamples.Key, out var statusResponse))
                    {
                        statusResponse = new Response();
                        operation.Responses[statusSamples.Key] = statusResponse;
                        if (isSuccessResponse)
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
