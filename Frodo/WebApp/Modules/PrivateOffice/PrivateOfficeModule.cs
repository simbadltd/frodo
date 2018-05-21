using System;
using System.Linq;
using System.Net;
using Frodo.Common;
using Frodo.Core;
using Frodo.Core.Repositories;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NodaTime;

namespace Frodo.WebApp.Modules.PrivateOffice
{
    public class PrivateOfficeModule : NancyModule
    {
        public PrivateOfficeModule(IRepository<User> userRepository, IRepository<ImportState> importStateRepository, IUnitOfWork unitOfWork) : base("/privateOffice")
        {
            this.RequiresAuthentication();

            Get["/"] = p =>
            {
                var user = userRepository.FindByLogin(Context.CurrentUser.UserName);
                return View["Index", user];
            };

            Get["/import"] = p =>
            {
                var importState = FetchImportState(userRepository, importStateRepository);

                return View["Import", ImportModel.FromImportState(importState)];
            };

            Post["/import"] = p =>
            {
                var model = this.Bind<ImportModel>();
                var importState = FetchImportState(userRepository, importStateRepository);

                importState.LastImportedDate =
                    OffsetDateTime.FromDateTimeOffset(DateTimeOffset.Parse(model.LastImportedDate));

                importStateRepository.Save(importState);
                unitOfWork.Commit();

                return View["Import", ImportModel.FromImportState(importState)];
            };
        }

        private ImportState FetchImportState(IRepository<User> userRepository, IRepository<ImportState> importStateRepository)
        {
            var user = userRepository.FindByLogin(Context.CurrentUser.UserName);
            var importState = importStateRepository.GetOrCreate(user);

            return importState;
        }

        public sealed class ImportModel
        {
            public string LastImportedDate { get; set; }

            public static ImportModel FromImportState(ImportState importState)
            {
                var model = new ImportModel
                {
                    LastImportedDate = importState.LastImportedDate.ToIso8601String()
                };

                return model;
            }
        }
    }
}