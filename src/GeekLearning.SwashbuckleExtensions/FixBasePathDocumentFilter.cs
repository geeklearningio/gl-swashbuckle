namespace GeekLearning.SwashbuckleExtensions
{
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

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
