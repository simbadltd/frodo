using System;
using System.Collections.Generic;
using Frodo.Composition;
using Frodo.Core;
using Frodo.Core.Repositories;
using Frodo.Infrastructure.Configuration;
using Frodo.Infrastructure.Ioc;
using Frodo.Infrastructure.Logging;
using Nancy.Hosting.Self;

namespace Frodo
{
    internal class WindowsService : IDisposable
    {
        private readonly IIocContainer _container;
        private readonly ILogger _logger;

        private bool _disposed;

        private NancyHost _host;

        public WindowsService()
        {
            _container = IocContainer.Compose();

            _logger = _container.Resolve<ILogManager>().GetLogger();

            // todo [kk]: fix mapping
            //_container.Resolve<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            var settings = _container.Resolve<ISettingsProvider>().Settings;
            var domain = settings.Domain;
            var port = settings.Port;

            var hostUrl = string.Format($"http://{domain}:{port}/");
            
            var config = new HostConfiguration { 
                UnhandledExceptionCallback = e => _logger.Error(e, "Unhandled exception caught.")
            };
            
            _host = new NancyHost(config, new Uri(hostUrl));
            _host.Start();

            _logger.Info("Application started");
            _logger.Info(hostUrl);

            AppDomain.CurrentDomain.UnhandledException +=
                (sender, e) => _logger.Error(e.ExceptionObject as Exception, "Unhandled exception caught.");

            var user = SeedUser();

            var importFeature = _container.Resolve<IImportFeature>();
            importFeature.Execute(user);

            var exportFeature = _container.Resolve<IExportFeature>();
            exportFeature.Execute(user);
        }

        private User SeedUser()
        {
            var newUser = CreateUser();
            var userRepository = _container.Resolve<IRepository<User>>();
            userRepository.DeleteAll();
            _container.Resolve<IUnitOfWork>().Commit();
            
            userRepository.Save(newUser);
            _container.Resolve<IUnitOfWork>().Commit();

            return newUser;
        }

        private static User CreateUser()
        {
            var usr = new User
            {
                Id = new Guid("1e865e7c-00ad-41e2-8627-1456276265f7"),

                TogglApiToken = "",

                JiraUserName = "",
                Email = "",
                JiraAccountPassword = "",
                Login = "tst",
                PasswordHash = "2B32548D3D46011CAEC455978738A30C",
                Salt = "test_salt",

                TaskPatterns = new List<string>
                {
                    "^(?<task>\\S*)(?<activity>) (?<content>.*)",
                    "^(?<task>\\S*)\\/(?<activity>\\S*) (?<content>.*)",
                },

                TaskIdMap = new Dictionary<string, string>
                {
                    {"MEET", "POL-23138"},
                    {"TL_ACT", "POL-12114"},
                    {"CR", "POL-12102"},
                    {"HLP_TEAM", "POL-12110"},
                    {"OFFICE", "HR-4"},
                    {"MGMNT", "POL-12109"},
                    {"OTH", "IN-2"},
                    {"WIKI", "POL-12100"},
                    {"DEMO", "POL-12096"},
                    {"1B1", "IN-54"},
                    {"PERF", "IN-6"},
                },

                ActivityMap = new Dictionary<string, Activity>
                {
                    {"anl", Activity.Analysis},
                    {"dev", Activity.Development},
                    {"tst", Activity.Testing},
                }
            };

            return usr;
        }

        public void Stop()
        {
            _host.Stop();

            _logger.Info("Application stopped");
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _host.Dispose();
                    _container.Dispose();
                }

                _disposed = true;
            }
        }

        ~WindowsService()
        {
            Dispose(false);
        }
    }
}