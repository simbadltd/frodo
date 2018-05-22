using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Frodo.Common;
using Frodo.Core;
using Frodo.Core.Repositories;
using Frodo.WebApp.Models;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using NodaTime;

namespace Frodo.WebApp.Modules.PrivateOffice
{
    public sealed class PrivateOfficeModule : NancyModule
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<ImportState> _importStateRepository;
        private readonly IRepository<TimeEntry> _timeEntryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PrivateOfficeModule(IRepository<User> userRepository, IRepository<ImportState> importStateRepository,
            IRepository<TimeEntry> timeEntryRepository, IUnitOfWork unitOfWork) : base("/privateOffice")
        {
            _userRepository = userRepository;
            _importStateRepository = importStateRepository;
            _timeEntryRepository = timeEntryRepository;
            _unitOfWork = unitOfWork;

            this.RequiresAuthentication();

            Get["/"] = p =>
            {
                var user = userRepository.FindByLogin(Context.CurrentUser.UserName);
                return View["Index", user];
            };

            Get["/import"] = p => BuildView();
            Post["/import"] = p =>
            {
                var model = this.Bind<ImportViewWriteState>();
                UpdateImportState(model);
                return BuildView();
            };
        }

        private Negotiator BuildView()
        {
            var user = _userRepository.FindByLogin(Context.CurrentUser.UserName);
            var importState = _importStateRepository.GetOrCreate(user);
            var timeEntries = _timeEntryRepository.NotExported(user);

            var model = new ImportViewReadState
            {
                LastImportedDate = importState.LastImportedDate.ToIso8601String(),
                TimeEntries = timeEntries.Select(TimeEntryDto.From).ToList()
            };

            return View["Import", model];
        }

        private void UpdateImportState(ImportViewWriteState model)
        {
            var user = _userRepository.FindByLogin(Context.CurrentUser.UserName);
            var importState = _importStateRepository.GetOrCreate(user);

            importState.LastImportedDate =
                OffsetDateTime.FromDateTimeOffset(DateTimeOffset.Parse(model.LastImportedDate));
            _importStateRepository.Save(importState);
            _unitOfWork.Commit();
        }

        public sealed class ImportViewReadState
        {
            public string LastImportedDate { get; set; }

            public List<TimeEntryDto> TimeEntries { get; set; }
        }

        public sealed class ImportViewWriteState
        {
            public string LastImportedDate { get; set; }
        }
    }
}