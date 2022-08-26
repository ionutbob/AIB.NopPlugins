using System;
using System.Linq.Dynamic.Core;
using System.Linq;
using Nop.Core.Data;
using Nop.Plugin.Misc.BlockMaliciousRequests.Domain;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Framework
{
    public class RemoveLogEntriesTask : IScheduleTask
    {
        private readonly IRepository<RequestLogRecord> _logRepository;

        public RemoveLogEntriesTask(IRepository<RequestLogRecord> logRepository)
        {
            _logRepository = logRepository;
        }

        public void Execute()
        {
            // delete all log entries older than 7 days
            var entriesToBeDeleted = _logRepository.Table.Where(log => log.RequestTime < DateTime.Now.AddDays(-7));
            _logRepository.Delete(entriesToBeDeleted);
        }
    }
}
