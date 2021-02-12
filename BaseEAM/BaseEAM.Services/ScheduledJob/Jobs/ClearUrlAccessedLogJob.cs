/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using BaseEAM.Data;
using Common.Logging;
using Quartz;
using System;
using System.Linq;

namespace BaseEAM.Services
{
    [DisallowConcurrentExecution]
    public class ClearUrlAccessedLogJob : IJob
    {
        private readonly IRepository<ActivityLog> _activityLogRepository;
        private readonly IDbContext _dbContext;
        private static readonly ILog s_log = LogManager.GetLogger<ClearUrlAccessedLogJob>();

        public ClearUrlAccessedLogJob(IRepository<ActivityLog> activityLogRepository,
            IDbContext dbContext)
        {
            this._activityLogRepository = activityLogRepository;
            this._dbContext = dbContext;
        }

        public void Execute(IJobExecutionContext context)
        {
            s_log.Info("ClearUrlAccessedLogJob START");

            var date = DateTime.UtcNow.AddDays(-1);
            var activityLogs = _activityLogRepository.GetAll()
                .Where(a => a.ActivityLogType.Name == "UrlAccessed" && a.ModifiedDateTime < date)
                .ToList();
            foreach (var log in activityLogs)
                _activityLogRepository.Delete(log);

            this._dbContext.SaveChanges();

            s_log.Info("ClearUrlAccessedLogJob END");
        }
    }
}
