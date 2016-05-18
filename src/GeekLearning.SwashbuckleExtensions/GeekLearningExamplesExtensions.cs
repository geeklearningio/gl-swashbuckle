using Swashbuckle.SwaggerGen;
using Swashbuckle.SwaggerGen.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekLearning.SwashbuckleExtensions
{
    public static class GeekLearningExamplesExtensions
    {
        public static void UseExamples(this SwaggerGenOptions documentOptions, Action<ExamplesBuilder> examplesBuilder)
        {
            var examples = new ExamplesBuilder();
            examplesBuilder(examples);
            documentOptions.OperationFilter<AssignExamples>(examples);
        }
    }
}
