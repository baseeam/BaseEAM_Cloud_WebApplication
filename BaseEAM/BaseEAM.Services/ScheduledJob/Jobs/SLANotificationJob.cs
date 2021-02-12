/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core;
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using BaseEAM.Data;
using Common.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseEAM.Services
{
    public class SLANotificationJob : IJob
    {
        private readonly IRepository<SLAInstance> _slaInstanceRepository;
        private readonly IWorkflowBaseService _workflowBaseService;
        private readonly IMessageService _messageService;
        private readonly IDbContext _dbContext;
        private static readonly ILog s_log = LogManager.GetLogger<SLANotificationJob>();

        public SLANotificationJob(IRepository<SLAInstance> slaInstanceRepository,
            IWorkflowBaseService workflowBaseService,
            IMessageService messageService,
            IDbContext dbContext)
        {
            this._slaInstanceRepository = slaInstanceRepository;
            this._workflowBaseService = workflowBaseService;
            this._messageService = messageService;
            this._dbContext = dbContext;
        }

        public void Execute(IJobExecutionContext context)
        {
            s_log.Info("SLANotificationJob START");

            var slaInstances = _slaInstanceRepository.GetAll()
                .Where(s => s.Violated == false && s.Closed == false)
                .ToList();
            foreach(var slaInstance in slaInstances)
            {
                bool instanceViolated = true;
                foreach (var instanceTerm in slaInstance.SLAInstanceTerms)
                {
                    var slaTerm = instanceTerm.SLATerm;

                    var trackingBaseDateTime = _workflowBaseService.GetDateTimeFromFieldName(
                        slaInstance.EntityId.Value, 
                        slaInstance.EntityType, 
                        slaTerm.TrackingBaseField
                        );

                    if(trackingBaseDateTime == null)
                    {
                        continue;
                    }

                    var trackingDateTime = _workflowBaseService.GetDateTimeFromFieldName(
                        slaInstance.EntityId.Value,
                        slaInstance.EntityType,
                        slaTerm.TrackingField
                        );

                    if(trackingDateTime == null)
                    {
                        trackingDateTime = DateTime.UtcNow;
                    }

                    var elapsed = trackingDateTime - trackingBaseDateTime;

                    // violated checking
                    if(elapsed.Value.Hours >= slaTerm.LimitHours && elapsed.Value.Minutes >= slaTerm.LimitMinutes)
                    {
                        instanceTerm.Violated = true;
                    }
                    else
                    {
                        instanceViolated = false;
                    }

                    // send notification
                    var sequences = slaTerm.NotificationSequences
                        .Where(n => elapsed.Value.Hours >= n.SendingTimeHours && elapsed.Value.Minutes >= n.SendingTimeMinutes)
                        .OrderByDescending(n => n.Sequence)
                        .ToList();
                    if(sequences.Count > 0)
                    {
                        var sequence = sequences[0];
                        BaseEntity entity = _dbContext.GetByEntityIdAndType(slaInstance.EntityId.Value, slaInstance.EntityType) as BaseEntity;
                        _messageService.SendMessage(entity, sequence.MessageTemplate, sequence.Users);
                    }
                    
                }
                slaInstance.Violated = instanceViolated;

                this._dbContext.SaveChanges();
            }

            s_log.Info("SLANotificationJob END");
        }
    }
}
