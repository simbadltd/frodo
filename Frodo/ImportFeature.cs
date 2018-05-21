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

            var importState = _importStateRepository.GetOrCreate(user);

            var start = importState.LastImportedDate;
            var end = OffsetDateTime.FromDateTimeOffset(DateTimeOffset.Now);

            _logger.Info($"Import period: {start} - {end}");
            var result = _timeEntriesImportService.Import(user, start, end);

            foreach (var timeEntry in result)
            {
                var tasks = _taskCommentParsingLogic.Extract(user, timeEntry.Description);

                foreach (var task in tasks)
                {
                    var clone = timeEntry.Clone();
                    clone.Description = task.Comment;
                    clone.TaskId = task.TaskId;
                    clone.Activity = task.Activity;

                    _logger.Debug($"Imported: {clone.TaskId} {clone.Activity} {clone.Description}");

                    var isTimeRedefinedByUser = task.RedefinedTime != null;
                    if (isTimeRedefinedByUser)
                    {
                        clone.Duration = task.RedefinedTime.ApplyToDuration(clone.Duration);
                        _logger.Debug(
                            $"Time for {clone.TaskId} has been redefined by user. Original:<{timeEntry.Duration}>, Current:<{clone.Duration}>");
                    }

                    _timeEntryRepository.Save(clone);
                }

                if (timeEntry.End.ToInstant() > importState.LastImportedDate.ToInstant())
                {
                    importState.LastImportedDate = timeEntry.End;
                }
            }

            _importStateRepository.Save(importState);
            _unitOfWork.Commit();

            _logger.Info("Import finished");
        }
    }
}