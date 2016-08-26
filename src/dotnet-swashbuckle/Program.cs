using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekLearning.DotNet.Swashbuckle
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var options = CommandLineOptions.Parse(args);


            var startup = Type.GetType(args[0]);
            var genericTestEnvironment = typeof(GeekLearning.Test.Integration.Environment.TestEnvironment<,>);
            var testEnvironmentType = genericTestEnvironment.MakeGenericType(startup, typeof(GeekLearning.Test.Integration.Environment.DefaultStartupConfigurationService));

            var testEnvironment = (GeekLearning.Test.Integration.Environment.ITestEnvironment)Activator.CreateInstance(testEnvironmentType);


        }
    }
}
