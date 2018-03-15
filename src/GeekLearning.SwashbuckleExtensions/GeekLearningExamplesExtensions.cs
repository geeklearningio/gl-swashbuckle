namespace GeekLearning.SwashbuckleExtensions
{
    using System;
    using System.Collections.Generic;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public static class GeekLearningExamplesExtensions
    {
        public static void UseExamples(this SwaggerGenOptions documentOptions, Action<ExamplesBuilder> examplesBuilder)
        {
            var examples = new ExamplesBuilder();
            examplesBuilder(examples);
            documentOptions.OperationFilter<AssignExamples>(examples);
        }

        public static void UseSchemes(this SwaggerGenOptions documentOptions, Action<ISet<string>> schemesBuilder)
        {
            var schemes = new HashSet<string>();
            schemesBuilder(schemes);
            documentOptions.DocumentFilter<SchemesDocumentFilter>(schemes);
        }
    }
}
