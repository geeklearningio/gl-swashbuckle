using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;
using Swashbuckle.AspNetCore.Swagger;

namespace GeekLearning.SwashbuckleExtensions
{
    public class XNullableFilter : ISchemaFilter
    {
        private readonly bool omitFalseValue;

        public XNullableFilter(bool omitFalseValue)
        {
            this.omitFalseValue = omitFalseValue;
        }

        public XNullableFilter() : this(true)
        {
        }

        public void Apply(Schema model, SchemaFilterContext context)
        {
            bool nullable = !context.SystemType.IsValueType;

            var type = context.SystemType;
            if (type.IsConstructedGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(Nullable<>))
                {
                    nullable = true;
                    //var nullableUnderlyingType = Nullable.GetUnderlyingType(context.SystemType);
                }
            }
           
            if (nullable || !omitFalseValue)
            {
                model.Extensions["x-nullable"] = nullable;
            }
        }
    }
}
