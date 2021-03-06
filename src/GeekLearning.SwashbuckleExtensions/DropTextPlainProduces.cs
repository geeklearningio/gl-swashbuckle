﻿namespace GeekLearning.SwashbuckleExtensions
{
    using System;
    using System.Linq;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

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
