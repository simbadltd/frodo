using System;
using Frodo.Core;
using Frodo.Core.Repositories;
using Frodo.Infrastructure.Logging;
using Frodo.Integration;

namespace Frodo
{
    public sealed class ExportFeature : IExportFeature
    {
        private readonly ITimeEntriesExportService _timeEntriesExportService;

        private readonly IRepository<TimeEntry> _timeEntryRepository;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger _logger;

        public ExportFeature(ITimeEntriesExportService timeEntriesExportService, IRepository<TimeEntry> timeEntryRepository, IUnitOfWork unitOfWork, ILogManager logManager)
        {
            _timeEntriesExportService = timeEntriesExportService;
            _timeEntryRepository = timeEntryRepository;
            _unitOfWork = unitOfWork;
            _logger = logManager.GetLogger();
        }

        public void Execute(User user)
        {
            _logger.Info("Export started");

            var entriesToExport = _timeEntryRepository.NotExported(user);
            _logger.Info($"{entriesToExport.Count} time entries to export");

            var successful = 0;
            var failed = 0;

            foreach (var timeEntry in entriesToExport)
            {
                if (timeEntry.IsExported)
                {
                    continue;
                }

                try
                {
                    timeEntry.MapTaskId(user);

                    _timeEntriesExportService.Export(user, timeEntry);
                    timeEntry.IsExported = true;
                    _timeEntryRepository.Save(timeEntry);
                    _unitOfWork.Commit();

                    _logger.Info(
                        $"{timeEntry.TaskId} [{timeEntry.Activity}]: {timeEntry.Description} ({timeEntry.Duration}) - OK");

                    successful++;
                }
                catch (Exception e)
                {
                    _logger.Info(
                        $"{timeEntry.TaskId} [{timeEntry.Activity}]: {timeEntry.Description} ({timeEntry.Duration}) - Fail");
                    _logger.Error(e,
                        $"Error while exporting {timeEntry.TaskId} [{timeEntry.Activity}]: {timeEntry.Description} ({timeEntry.Duration})");

                    failed++;
                }
            }

            _logger.Info($"Export finished [OK: {successful}; Failed:{failed}].");
        }
    }
}