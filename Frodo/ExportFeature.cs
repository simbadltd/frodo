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

            foreach (var timeEntry in entriesToExport)
            {
                timeEntry.MapTaskId(user);
                var isSuccessful = _timeEntriesExportService.Export(user, timeEntry);

                if (isSuccessful)
                {
                    timeEntry.IsExported = true;
                    _timeEntryRepository.Save(timeEntry);
                    _unitOfWork.Commit();

                    _logger.Debug(
                        $"Export: {timeEntry.TaskId} {timeEntry.Activity} {timeEntry.Description} successful");
                }
                else
                {
                    _logger.Error($"Export: {timeEntry.TaskId} {timeEntry.Activity} {timeEntry.Description} failed");
                }
            }

            _logger.Info("Export finished");
        }
    }
}