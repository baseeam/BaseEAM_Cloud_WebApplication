/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using BaseEAM.Data;
using Common.Logging;
using Quartz;

namespace BaseEAM.Services
{
    public class ExecuteActionJob : IJob
    {
        private static readonly ILog s_log = LogManager.GetLogger<ExecuteActionJob>();
        private readonly IRepository<AutomatedAction> _automatedActionRepository;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly IDbContext _dbContext;

        public ExecuteActionJob(IRepository<AutomatedAction> automatedActionRepository,
            IUserService userService,
            IMessageService messageService,
            IDbContext dbContext)
        {
            this._automatedActionRepository = automatedActionRepository;
            this._userService = userService;
            this._messageService = messageService;
            this._dbContext = dbContext;
        }

        public void Execute(IJobExecutionContext context)
        {
            s_log.Info("ExecuteActionJob START");

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            long entityId = dataMap.GetLong("EntityId");
            string entityType = dataMap.GetString("EntityType");
            long automatedActionId = dataMap.GetLong("AutomatedActionId");
            var ac = _automatedActionRepository.GetById(automatedActionId);
            var wfEntity = this._dbContext.GetByEntityIdAndType(entityId, entityType) as WorkflowBaseEntity;
            if(ac.ActionType.Name == "Send Message")
            {
                SendMessage(ac, wfEntity);
            }
            else if (ac.ActionType.Name == "Launch Workflow")
            {

            }
            else if (ac.ActionType.Name == "Update Field Value")
            {

            }

            s_log.Info("ExecuteActionJob END");
        }

        private void SendMessage(AutomatedAction ac, WorkflowBaseEntity wfEntity)
        {
            var users = _userService.GetUsers(ac.Users, wfEntity);
            //Notify users
            if (!string.IsNullOrEmpty(ac.MessageTemplate))
            {
                _messageService.SendMessage(wfEntity, ac.MessageTemplate, users, null);
            }
        }

        private void LaunchWorkflow(AutomatedAction ac, WorkflowBaseEntity wfEntity)
        {

        }
    }
}
