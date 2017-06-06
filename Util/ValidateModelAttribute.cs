using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Pdf.Service.Util
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var modelState = context.ModelState;

            if (!modelState.IsValid)
            {
                var data = modelState.Where(x => x.Value.ValidationState == ModelValidationState.Invalid)
                    .Select(x => new ApiError("INVALID_MODEL", ParseMessage(x)));

                context.Result = new ContentResult
                {
                    Content = JsonConvert.SerializeObject(data),
                    StatusCode = 400,
                    ContentType = "application/json"
                };
            }
            base.OnActionExecuting(context);
        }

        private static string ParseMessage(System.Collections.Generic.KeyValuePair<string, ModelStateEntry> x)
        {
            return x.Value.Errors
                .Select(e => e.ErrorMessage)
                .Aggregate("", (a, b) => $"{a}, {x.Key}: {b}").Trim(',').Trim();
        }
    }

}