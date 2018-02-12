using System;
using System.Linq;
using Frodo.Core;
using Frodo.Core.Repositories;
using Frodo.Infrastructure.Logging;
using Frodo.Integration;
using NodaTime;

namespace Frodo
{
    public sealed class ImportFeature : IImportFeature
    {
        private readonly ITimeEntriesImportService _timeEntriesImportService;

        private readonly ITaskCommentParsingLogic _taskCommentParsingLogic;

        private readonly IRepository<ImportState> _importStateRepository;

        private readonly IRepository<TimeEntry> _timeEntryRepository;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger _logger;

        public ImportFeature(ITimeEntriesImportService timeEntriesImportService, ITaskCommentParsingLogic taskCommentParsingLogic, IRepository<ImportState> importStateRepository, IRepository<TimeEntry> timeEntryRepository, IUnitOfWork unitOfWork, ILogManager logManager)
        {
            _timeEntriesImportService = timeEntriesImportService;
            _taskCommentParsingLogic = taskCommentParsingLogic;
            _importStateRepository = importStateRepository;
            _timeEntryRepository = timeEntryRepository;
            _unitOfWork = unitOfWork;
            _logger = logManager.GetLogger();
        }

        public void Execute(User user)
        {
            _logger.Info("Import started");

            var importState = _importStateRepository.FindSingle(x => x.UserId == user.Id) ?? new ImportState
            {
                UserId = user.Id,
                LastImportedDate = OffsetDateTime.FromDateTimeOffset(DateTimeOffset.Now.AddDays(-2)),
            };

            var start = importState.LastImportedDate;
            var end = OffsetDateTime.FromDateTimeOffset(DateTimeOffset.Now);

            _logger.Info($"Import period: {start} - {end}");
            var result = _timeEntriesImportService.Import(user, start, end);

            foreach (var timeEntry in result)
            {
                var comment = _taskCommentParsingLogic.Extract(user, timeEntry.Description);

                timeEntry.Description = comment.Content;
                timeEntry.TaskId = comment.TaskId;
                timeEntry.Activity = comment.Activity;

                _logger.Debug($"Imported: {timeEntry.TaskId} {timeEntry.Activity} {timeEntry.Description}");

                if (timeEntry.End.ToInstant() > importState.LastImportedDate.ToInstant())
                {
                    importState.LastImportedDate = timeEntry.End;
                }

                _timeEntryRepository.Save(timeEntry);
            }

            _importStateRepository.Save(importState);
            _unitOfWork.Commit();

            _logger.Info("Import finished");
        }
    }
}