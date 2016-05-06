using Swashbuckle.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekLearning.SwashbuckleExtensions
{
    public static class GeekLearningExamplesExtensions
    {
        public static void UseExamples(this SwaggerDocumentOptions documentOptions, Action<AssignExamples> examplesBuilder)
        {
            var examples = new AssignExamples();
            examplesBuilder(examples);
            documentOptions.OperationFilter(examples);
        }
    }
}
