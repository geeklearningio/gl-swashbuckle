using Swashbuckle;
using Swashbuckle.SwaggerGen;
using Swashbuckle.SwaggerGen.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekLearning.SwashbuckleExtensions
{
    public class SchemesDocumentFilter : IDocumentFilter
    {
        public SchemesDocumentFilter()
        {
            this.Schemes = new List<string>();
        }

        public SchemesDocumentFilter(params string[] schemes)
        {
            this.Schemes = schemes;
        }

        public IList<string> Schemes { get; set; }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Schemes = this.Schemes;
        }
    }
}
