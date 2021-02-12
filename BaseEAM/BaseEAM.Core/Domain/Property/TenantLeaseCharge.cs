/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System;
using System.ComponentModel.DataAnnotations;

namespace BaseEAM.Core.Domain
{
    public class TenantLeaseCharge : BaseEntity
    {
        public long? TenantLeaseId { get; set; }
        public virtual TenantLease TenantLease { get; set; }

        public long? ChargeTypeId { get; set; }
        public virtual ValueItem ChargeType { get; set; }

        public decimal? ChargeAmount { get; set; }
        public bool AmountIsOverridable { get; set; }
        public int? ChargeDueType { get; set; }
        public DateTime? ChargeDueDate { get; set; }
        public int? ChargeDueDay { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    public enum ChargeDueType
    {
        [Display(Name = "Each time rent is due")]
        EachTimeRentIsDue,

        [Display(Name = "Once only, when the lease starts")]
        OnceOnlyWhenTheLeaseStarts,

        [Display(Name = "Once only, when the lease ends")]
        OnceOnlyWhenTheLeaseEnds,

        [Display(Name = "Once only, on a specific date:")]
        OnceOnlyOnaSpecificDate,

        [Display(Name = "Monthly, on a specific day:")]
        MonthlyOnaSpecificDay,
    }
}
