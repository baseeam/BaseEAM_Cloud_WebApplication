/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core;
using BaseEAM.Core.Domain;
using BaseEAM.Core.Kendoui;
using System;
using System.Collections.Generic;

namespace BaseEAM.Services
{
    public interface ITenantLeaseService : IBaseService
    {
        PagedResult<TenantLease> GetTenantLeases(string expression,
           dynamic parameters,
           int pageIndex = 0,
           int pageSize = 2147483647,
           IEnumerable<Sort> sort = null); //Int32.MaxValue

        List<User> GetCreatedUser(long id);

        /// <summary>
        /// Create a new Rent Payment
        /// </summary>
        /// <param name="tenantLeasePaymentSchedule"></param>
        /// <param name="startDate"></param>
        void CreateRentPayment(TenantLeasePaymentSchedule tenantLeasePaymentSchedule, DateTime? startDate);

        /// <summary>
        /// Generate the list of payment schedules
        /// </summary>
        /// <param name="tenantLease"></param>
        void GeneratePaymentSchedules(TenantLease tenantLease);

        /// <summary>
        /// Create a new Charge Payment
        /// </summary>
        /// <param name="tenantLeasePaymentSchedule"></param>
        /// <param name="startDate"></param>
        void CreateChargePayment(TenantLeaseCharge tenantLeaseCharge, DateTime? dueDate);

        DateTime? GetDueDate(TenantLease tenantLease, TenantLeaseCharge tenantLeaseCharge);

        /// <summary>
        /// This method will be used in the background job to generate payments for next period
        /// It will only Add new Payments of Rent & Charge for next period
        /// </summary>
        void UpdatePayment(TenantLease tenantLease);

    }
}
