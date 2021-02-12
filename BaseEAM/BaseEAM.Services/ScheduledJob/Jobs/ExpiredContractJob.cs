/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using Common.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseEAM.Services
{
    [DisallowConcurrentExecution]
    public class ExpiredContractJob : IJob
    {
        private readonly IRepository<Contract> _contractRepository;
        private readonly IContractService _contractService;

        private static readonly ILog s_log = LogManager.GetLogger<ExpiredContractJob>();

        public ExpiredContractJob(IRepository<Contract> contractRepository, IContractService contractService)
        {
            this._contractRepository = contractRepository;
            this._contractService = contractService;
        }

        public void Execute(IJobExecutionContext context)
        {
            s_log.Info("ExpiredContractJob START");

            var approvedContracts = _contractRepository.GetAll().Where(c => c.Assignment.Name == "Approved" && DateTime.Now >= c.EndDate).ToList();

            if (approvedContracts.Count == 0)
                return;
            var contracts = new List<Contract>();
            foreach (var contract in approvedContracts)
            {
                contracts.Add(contract);
            }
            this._contractService.UpdateExpiredContracts(contracts);

            s_log.Info("ExpiredContractJob END");
        }
    }
}
