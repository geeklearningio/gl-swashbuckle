namespace GeekLearning.SwashbuckleExtensions
{
    using System;

    /// <summary>
    /// Thanks to jonnybi - https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/193
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class SwaggerFormParameterAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }

        public SwaggerFormParameterAttribute(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
