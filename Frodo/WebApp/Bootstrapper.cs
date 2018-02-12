using System.Collections.Generic;
using Frodo.Infrastructure.Ioc;
using Frodo.WebApp.Authentication;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Responses;
using Nancy.Session;
using Nancy.Session.InProc;
using Nancy.ViewEngines.Razor;

namespace Frodo.WebApp
{
    public sealed class Bootstrapper : IocContainerNancyBootstrapper
    {
        protected override void ApplicationStartup(IIocContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            Nancy.Json.JsonSettings.MaxJsonLength = int.MaxValue;
            pipelines.EnableInProcSessions();
        }

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            conventions.ViewLocationConventions.Add((viewName, model, context) =>
                string.Concat("WebApp/Views/", context.ModuleName, "/", viewName));

            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/images", "/Images", ".png", ".gif"));
            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/fonts", "/Fonts",
                ".ttf", ".eot", ".svg", ".woff", ".woff2"));
            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/scripts", "/Scripts", ".js"));
            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/app", "/App", ".js",
                ".html"));

            base.ConfigureConventions(conventions);
        }

        protected override void RequestStartup(IIocContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            var formsAuthConfiguration = new FormsAuthenticationConfiguration
            {
                RedirectUrl = "~/login",
                UserMapper = container.Resolve<IUserMapper>(),
            };

            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
        }
    }
    
    public class RazorConfig : IRazorConfiguration
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            yield return "Frodo.Core";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "Frodo.Core";
        }

        public bool AutoIncludeModelNamespace => true;
    }    
}