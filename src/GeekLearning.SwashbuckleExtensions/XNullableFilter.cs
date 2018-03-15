namespace GeekLearning.SwashbuckleExtensions
{
    using System;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

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
            var nullable = !context.SystemType.IsValueType;

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

            if (nullable || !this.omitFalseValue)
            {
                model.Extensions["x-nullable"] = nullable;
            }
        }
    }
}
