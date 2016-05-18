using Swashbuckle.SwaggerGen;
using Swashbuckle.SwaggerGen.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace GeekLearning.SwashbuckleExtensions
{
    public class AssignSecurityRequirements : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {

            var controllerAuthorize = context.ApiDescription.GetControllerAttributes()
                .OfType<AuthorizeAttribute>();

            var actionAttributes = context.ApiDescription.GetActionAttributes();

            var actionAuthorize = actionAttributes.OfType<AuthorizeAttribute>();

            var actionAnonymous = actionAttributes.OfType<AllowAnonymousAttribute>();

            // Correspond each "Authorize" role to an oauth2 scope
            var controllerScopes = controllerAuthorize
                .SelectMany(attr => (attr.Roles ?? "").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));

            var actionScopes = actionAuthorize
                .SelectMany(attr => (attr.Roles ?? "").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));

            var scopes = controllerScopes.Union(actionScopes).Distinct();

            var securityMethods = controllerAuthorize.Concat(actionAuthorize).GroupBy(x => x.Policy);

            if (securityMethods.Any())
            {
                foreach (var security in securityMethods)
                {
                    if (operation.Security == null)
                        operation.Security = new List<IDictionary<string, IEnumerable<string>>>();

                    var oAuthRequirements = new Dictionary<string, IEnumerable<string>>
                    {
                        { security.Key , scopes  }
                    };

                    operation.Security.Add(oAuthRequirements);
                }
            }

        }
    }
}
