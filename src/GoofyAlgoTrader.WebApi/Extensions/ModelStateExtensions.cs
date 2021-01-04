using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.WebApi
{
    public class ModelState
    {
        public string Name { get; set; }
        public string Message { get; set; }
    }

    public static class ModelStateExtensions
    {
        public static List<ModelState> GetValidationSummary(this ModelStateDictionary modelState)
        {
            if (modelState.IsValid) return null;

            var error = new List<ModelState>();

            foreach (var item in modelState)
            {
                var state = item.Value;
                var message = state.Errors.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.ErrorMessage))?.ErrorMessage;
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = state.Errors.FirstOrDefault(o => o.Exception != null)?.Exception.Message;
                }
                if (string.IsNullOrWhiteSpace(message)) continue;

                error.Add(new ModelState()
                {
                    Name = item.Key,
                    Message = message
                });
            }

            return error;
        }


        public static List<ModelState> GetModelStateError(string name, string msg)
        {
            return new List<ModelState>()
            {
                new ModelState(){
                    Name=name,
                    Message=msg
                }
            };
        }

        public static List<ModelState> GetModelStateError(Dictionary<string, string> dic)
        {
            var error = new List<ModelState>();

            foreach (var item in dic)
            {
                error.Add(new ModelState()
                {
                    Name = item.Key,
                    Message = item.Value
                });
            }

            return error;
        }
    }
}
