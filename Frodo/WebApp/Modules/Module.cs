using System.Collections.Generic;
using Nancy;
using Nancy.Validation;

namespace Frodo.WebApp.Modules
{
    public abstract class Module : NancyModule
    {
        protected IDictionary<string, List<string>> Errors { get; } 
        
        protected Module(string modulePath): base(modulePath)
        {
            Errors = new Dictionary<string, List<string>>();
            After.AddItemToEndOfPipeline(ctx => { ctx.ViewBag.Errors = Errors; });
        }
        
        protected bool Validate<T>(T instance)
        {
            var result = ModuleExtensions.Validate(this, instance);

            foreach (var error in result.Errors)
            {
                var list = Errors[error.Key] ?? new List<string>();
                Errors[error.Key] = list;

                foreach (var concreteError in error.Value)
                {
                    list.Add(concreteError.ErrorMessage);
                }
            }

            return result.IsValid;
        }

        protected void AddError(string key, string errorMessage)
        {
            if (Errors.ContainsKey(key) == false || Errors[key] == null)
            {
                Errors[key] = new List<string>();
            }
            
            Errors[key].Add(errorMessage);
        }
    }
}