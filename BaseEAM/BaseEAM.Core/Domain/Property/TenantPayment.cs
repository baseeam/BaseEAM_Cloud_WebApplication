/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System;
using System.Collections.Generic;

namespace BaseEAM.Core.Domain
{
    public class TenantPayment : BaseEntity
    {
        public long? SiteId { get; set; }
        public virtual Site Site { get; set; }

        public long? TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }

        public long? PropertyId { get; set; }
        public virtual Property Property { get; set; }

        public long? TenantLeaseId { get; set; }
        public virtual TenantLease TenantLease { get; set; }

        public long? TenantLeasePaymentScheduleId { get; set; }
        public virtual TenantLeasePaymentSchedule TenantLeasePaymentSchedule { get; set; }

        public long? TenantLeaseChargeId { get; set; }
        public virtual TenantLeaseCharge TenantLeaseCharge { get; set; }

        public DateTime? DueDate { get; set; }

        public long? ChargeTypeId { get; set; }
        public virtual ValueItem ChargeType { get; set; }

        public decimal? DueAmount { get; set; }
        public int DaysLateCount { get; set; }
        public decimal? LateFeeAmount { get; set; }
        public decimal? CollectedAmount { get; set; }
        public decimal? BalanceAmount { get; set; }

        private ICollection<TenantPaymentCollection> _tenantPaymentCollections;
        public virtual ICollection<TenantPaymentCollection> TenantPaymentCollections
        {
            get { return _tenantPaymentCollections ?? (_tenantPaymentCollections = new List<TenantPaymentCollection>()); }
            protected set { _tenantPaymentCollections = value; }
        }
    }
}
