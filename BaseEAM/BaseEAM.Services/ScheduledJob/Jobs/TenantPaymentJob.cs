﻿/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using Common.Logging;
using Quartz;
using System;
using System.Linq;

namespace BaseEAM.Services
{
    [DisallowConcurrentExecution]
    public class TenantPaymentJob : IJob
    {
        private static readonly ILog s_log = LogManager.GetLogger<TenantPaymentJob>();
        private readonly IRepository<TenantPayment> _tenantPaymentRepository;
        private readonly IRepository<TenantLease> _tenantLeaseRepository;
        private readonly ITenantLeaseService _tenantLeaseService;

        public TenantPaymentJob(IRepository<TenantPayment> tenantPaymentRepository,
        IRepository<TenantLease> tenantLeaseRepository,
        ITenantLeaseService tenantLeaseService)
        {
            this._tenantPaymentRepository = tenantPaymentRepository;
            this._tenantLeaseRepository = tenantLeaseRepository;
            this._tenantLeaseService = tenantLeaseService;
        }

        public void Execute(IJobExecutionContext context)
        {
            s_log.Info("TenantPaymentJob START");

            var tenantLeases = _tenantLeaseRepository.GetAll().Where(l => l.Assignment.Name == "Leasing").ToList();
            foreach (var tenantLease in tenantLeases)
            {
                //count list of payment which is generated by this tenantLease
                var countRentPayment = _tenantPaymentRepository.GetAll().Where(p => p.TenantLeaseId == tenantLease.Id).Count();
                _tenantLeaseService.UpdatePayment(tenantLease);
            }

            s_log.Info("TenantPaymentJob END");
        }
    }
}
