using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GeekLearning.SwashbuckleExtensions
{
    public class XMsEnumFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            var typeInfo = context.SystemType.GetTypeInfo();

            if (typeInfo.IsEnum)
            {
                var names = Enum.GetNames(context.SystemType);
                var values = model.Enum;

                model.Extensions.Add(
                    "x-ms-enum",
                    new
                    {
                        name = typeInfo.Name,
                        modelAsString = values.First() is string,
                        values = Enumerable
                            .Range(0, names.Length)
                            .Select(index => new
                            {
                                value = values[index],
                                name = names[index]
                            })
                    });
            }
        }
    }
}
