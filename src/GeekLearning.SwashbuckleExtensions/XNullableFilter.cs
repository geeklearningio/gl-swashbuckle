using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;
using Swashbuckle.AspNetCore.Swagger;

namespace GeekLearning.SwashbuckleExtensions
{
    public class XNullableFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            var nullableUnderlyingType = Nullable.GetUnderlyingType(context.SystemType);

            model.Extensions.Add("x-nullable", nullableUnderlyingType != null || !context.SystemType.IsValueType);
        }
    }
}
