using System;
using System.Collections.Generic;
using System.IO;
using Frodo.Common;
using Frodo.Composition;
using Frodo.Core;
using Frodo.Core.Repositories;
using Frodo.Infrastructure.Configuration;
using Frodo.Infrastructure.Ioc;
using Frodo.Infrastructure.Json;
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
            var newUser = CreateUserFromConfig();
            var userRepository = _container.Resolve<IRepository<User>>();
            userRepository.DeleteAll();
            _container.Resolve<IUnitOfWork>().Commit();

            userRepository.Save(newUser);
            _container.Resolve<IUnitOfWork>().Commit();

            return newUser;
        }

        private User CreateUserFromConfig()
        {
            var userConfigFilePath = PathUtils.ToAbsolutePath("~\\eap_user.json");
            if (File.Exists(userConfigFilePath) == false)
            {
                throw new FileNotFoundException($"Please, create user config file <{userConfigFilePath}>");
            }

            var jsonSerializer = _container.Resolve<IJsonSerializer>();
            var userConfig = jsonSerializer.Deserialize<UserConfig>(File.ReadAllText(userConfigFilePath));

            var usr = new User
            {
                Id = new Guid("1e865e7c-00ad-41e2-8627-1456276265f7"),
                TogglApiToken = userConfig.TogglApiToken,
                JiraUserName = userConfig.JiraUserName,
                Email = userConfig.Email,
                JiraAccountPassword = userConfig.JiraAccountPassword,
                Login = "tst",
                PasswordHash = "2B32548D3D46011CAEC455978738A30C",
                Salt = "test_salt",

                TaskIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "MEET", "POL-23138"
                    }, // Time spent on any meetings regarding this project. Daily, planning, retrospective, post-mortem, kick-off, demo. Or common project communications not specific task related. Comment is required!
                    {
                        "GMEET", "POL-31725"
                    }, // Common product meetings (not project specific). Discuss things not your task/project related. Comment is required!
                    {"CMEET", "IN-59"}, // General company meeting, cross-project meetings, workshops
                    {
                        "TL_ACT", "POL-12114"
                    }, // Processing emails, TPRs, unexpected request for some tasks analysis, resolving some problem etc. Comment is required!
                    {"CR", "POL-12102"}, // Any time spent on Code Review (not on fixes).
                    {"OFFICE", "HR-4"},
                    {"OTH", "IN-2"}, // Comment for this task is required!
                    {
                        "WIKI", "POL-12100"
                    }, // Time spent on updating wiki documentation in general, not related to specific task.
                    {"DEMO", "POL-12096"},
                    {"1B1", "IN-54"},
                    {"1T1", "IN-54"},
                    {"PERF", "IN-6"},
                    {"PR", "IN-6"},
                    {
                        "PLAN", "POL-14306"
                    }, // Time spent on prioritization / estimation / decomposition. Comment is required!
                    {
                        "EMERG", "POL-20874"
                    }, // Time spent on fixing emergencies (out-of-hours). Real emergency managed by responsible PM.
                    {"NEWS", "POL-12101"}, // Time spent on reading emails on Product changes, etc.
                    {
                        "ENV", "IN-9"
                    } // Setup environment from scratch / adding some tool for work (VS, SSRS etc), it is not related to specific project task and env preparation.
                },

                ActivityMap = new Dictionary<string, Activity>
                {
                    {"anl", Activity.Analysis},
                    {"dev", Activity.Development},
                    {"tst", Activity.Testing},
                    {"cr", Activity.CodeReview},
                    {"req", Activity.PM_Initiation_WorkWithRequirements},
                    {"plan", Activity.PM_Initiation_ProjectPlanning}, {"pp", Activity.PM_Initiation_ProjectPlanning},
                    {"daily", Activity.PM_Monitoring_DailyMeeting},
                    {"upd", Activity.PM_Monitoring_StatusUpdate},
                    {"com", Activity.PM_Monitoring_Communications},
                    {"retro", Activity.PM_ClosingRetrospective},
                    {"replan", Activity.PM_Unexpected_ProjectReplanning},
                }
            };

            if (userConfig.TaskIdMap != null)
            {
                foreach (var entry in userConfig.TaskIdMap)
                {
                    usr.TaskIdMap[entry.Key] = entry.Value;
                }
            }

            if (userConfig.ActivityMap != null)
            {
                foreach (var entry in userConfig.ActivityMap)
                {
                    usr.ActivityMap[entry.Key] = entry.Value;
                }
            }

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

        private sealed class UserConfig
        {
            public string TogglApiToken { get; set; }

            public string JiraUserName { get; set; }

            public string Email { get; set; }

            public string JiraAccountPassword { get; set; }

            public Dictionary<string, string> TaskIdMap { get; set; }

            public Dictionary<string, Activity> ActivityMap { get; set; }
        }
    }
}