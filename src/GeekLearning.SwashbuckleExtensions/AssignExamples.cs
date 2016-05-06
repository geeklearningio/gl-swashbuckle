using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Swashbuckle.SwaggerGen;

namespace GeekLearning.SwashbuckleExtensions
{
    public class AssignExamples : IOperationFilter
    {

        private Dictionary<string, Dictionary<string, Dictionary<string, object>>> examples = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();

        public AssignExamples()
        {
        }

        public ControllerContext<TController> ForController<TController>()
        {
            return new ControllerContext<TController>(examples);
        }

        public class ControllerContext<TController>
        {
            private Dictionary<string, Dictionary<string, Dictionary<string, object>>> examples;

            public ControllerContext(Dictionary<string, Dictionary<string, Dictionary<string, object>>> examples)
            {
                this.examples = examples;
            }

            public ActionContext<TReturn> ForAction<TReturn>(Expression<Func<TController, TReturn>> action, Action<ActionContext<TReturn>> builder)
            {
                var context = ForAction(action);
                builder(context);
                return context;
            }


            public ActionContext<TReturn> ForAction<TReturn>(Expression<Func<TController, TReturn>> action)
            {
                var methodCall = ((MethodCallExpression)action.Body);

                var actionId = methodCall.Method.DeclaringType.FullName + "." + methodCall.Method.Name;

                Dictionary<string, Dictionary<string, object>> actionResponses;

                if (!examples.TryGetValue(actionId, out actionResponses))
                {
                    actionResponses = new Dictionary<string, Dictionary<string, object>>();
                    examples[actionId] = actionResponses;
                }

                return new ActionContext<TReturn>(actionResponses);
            }


            public ActionContext<TReturn> ForAction<TReturn>(Expression<Func<TController, Task<TReturn>>> action, Action<ActionContext<TReturn>> builder)
            {
                var context = ForAction(action);
                builder(context);
                return context;
            }

            public ActionContext<TReturn> ForAction<TReturn>(Expression<Func<TController, Task<TReturn>>> action)
            {
                var methodCall = ((MethodCallExpression)action.Body);

                var actionId = methodCall.Method.DeclaringType.FullName + "." + methodCall.Method.Name;

                Dictionary<string, Dictionary<string, object>> actionResponses;

                if (!examples.TryGetValue(actionId, out actionResponses))
                {
                    actionResponses = new Dictionary<string, Dictionary<string, object>>();
                    examples[actionId] = actionResponses;
                }

                return new ActionContext<TReturn>(actionResponses);
            }

            public class ActionContext<TReturn>
            {
                private Dictionary<string, Dictionary<string, object>> actionResponses;

                public ActionContext(Dictionary<string, Dictionary<string, object>> actionResponses)
                {
                    this.actionResponses = actionResponses;
                }

                public ActionContext<TReturn> Json(TReturn data, string status = "200")
                {
                    Dictionary<string, object> statusResponses;

                    if (!actionResponses.TryGetValue(status, out statusResponses))
                    {
                        statusResponses = new Dictionary<string, object>();
                        actionResponses[status] = statusResponses;
                    }

                    statusResponses.Add("application/json", data);
                    return this;
                }
            }
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {

            Response successResponse;


            if (operation.Responses.TryGetValue("200", out successResponse) || operation.Responses.TryGetValue("204", out successResponse))
            {
            }


            Dictionary<string, Dictionary<string, object>> actionSamples;

            if (this.examples.TryGetValue(context.ApiDescription.ActionDescriptor.DisplayName, out actionSamples))
            {
                foreach (var statusSamples in actionSamples)
                {
                    Response statusResponse;
                    if (!operation.Responses.TryGetValue(statusSamples.Key, out statusResponse))
                    {
                        statusResponse = new Response();
                        operation.Responses[statusSamples.Key] = statusResponse;
                        if (successResponse != null)
                        {
                            statusResponse.Schema = successResponse.Schema;
                        }
                    }
                    statusResponse.Examples = statusSamples.Value;
                }
            }
        }
    }
}
