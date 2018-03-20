namespace GeekLearning.SwashbuckleExtensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class SchemesDocumentFilter : IDocumentFilter
    {
        public SchemesDocumentFilter(IEnumerable<string> schemes)
        {
            this.Schemes = schemes.ToArray();
        }

        public IList<string> Schemes { get; set; }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Schemes = this.Schemes;
        }
    }
}
