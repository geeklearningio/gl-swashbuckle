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
