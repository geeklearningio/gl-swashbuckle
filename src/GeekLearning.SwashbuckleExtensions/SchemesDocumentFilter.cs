namespace GeekLearning.SwashbuckleExtensions
{
    using Swashbuckle.Swagger.Model;
    using Swashbuckle.SwaggerGen.Generator;
    using System.Collections.Generic;
    using System.Linq;

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
