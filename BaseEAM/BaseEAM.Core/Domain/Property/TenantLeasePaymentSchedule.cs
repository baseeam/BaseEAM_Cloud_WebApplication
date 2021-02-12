/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System;

namespace BaseEAM.Core.Domain
{
    public class TenantLeasePaymentSchedule : BaseEntity
    {
        public long? TenantLeaseId { get; set; }
        public virtual TenantLease TenantLease { get; set; }

        public decimal? DueAmount { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
