using System.Collections.Generic;
using Frodo.Infrastructure.Ioc;
using Frodo.WebApp.Authentication;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Cryptography;
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

            var cryptographyConfiguration = new CryptographyConfiguration(
                new RijndaelEncryptionProvider(new PassphraseKeyGenerator("3101f2d9f7d04558b0e0798260ef7d17", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })),
                new DefaultHmacProvider(new PassphraseKeyGenerator("82faa2dff2ed41b4af4e970494f3e4a9", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })));

            var formsAuthConfiguration =
                new FormsAuthenticationConfiguration
                {
                    CryptographyConfiguration = cryptographyConfiguration,
                    RedirectUrl = "~/login",
                    UserMapper = container.Resolve<IUserMapper>(),
                };

            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
        }

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            conventions.ViewLocationConventions.Add((viewName, model, context) =>
                string.Concat("WebApp/Views/", context.ModuleName, "/", viewName));

            conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("/images", "/Images", ".png", ".gif"));
            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/fonts", "/Fonts",
                ".ttf", ".eot", ".svg", ".woff", ".woff2"));
            conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("/scripts", "/Scripts", ".js"));
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
            yield return "Frodo";
            yield return "Frodo.Core";
            yield return "NodaTime";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "Frodo";
            yield return "Frodo.Core";
            yield return "NodaTime";
        }

        public bool AutoIncludeModelNamespace => true;
    }
}