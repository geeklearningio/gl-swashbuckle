using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;
using Swashbuckle.AspNetCore.Swagger;

namespace GeekLearning.SwashbuckleExtensions
{
    public class FixBasePathDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.BasePath == "/")
            {
                swaggerDoc.BasePath = null;
            }
        }
    }
}
