namespace GeekLearning.SwashbuckleExtensions
{
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System;
    using System.Linq;

    public class DropTextPlainProduces : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Produces.Count > 0)
            {
                var textPlain = operation.Produces
                    .FirstOrDefault(produce => produce.Equals("text/plain", StringComparison.OrdinalIgnoreCase));

                if (textPlain != null)
                {
                    operation.Produces.Remove(textPlain);
                }
            }
        }
    }
}
