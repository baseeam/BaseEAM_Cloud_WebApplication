/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System;
using System.Collections.Generic;

namespace BaseEAM.Core.Domain
{
    public class TenantLease : WorkflowBaseEntity
    {
        public long? SiteId { get; set; }
        public virtual Site Site { get; set; }

        public long? TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }

        public long? PropertyId { get; set; }
        public virtual Property Property { get; set; }

        // Term
        public DateTime? TermStartDate { get; set; }
        public DateTime? TermEndDate { get; set; }
        public int? TermNumber { get; set; }
        public int? TermPeriod { get; set; }
        public bool TermIsMonthToMonth { get; set; }
        public decimal? TermRentAmount { get; set; }
        public int? DueFrequency { get; set; }

        public int? BiMonthlyStart { get; set; }
        public int? BiMonthlyEnd { get; set; }

        public DateTime? FirstPaymentDate { get; set; }

        // Late Fee
        public bool LateFeeEnabled { get; set; }
        public int? GracePeriodDays { get; set; }
        public int? LateFeeOption { get; set; }
        public decimal? FlatFee { get; set; }
        public decimal? BaseAmountPerDay { get; set; }
        public decimal? AmountPerDay { get; set; }
        public decimal? PercentOfRentPerDay { get; set; }
        public bool StopPerDay { get; set; }
        public decimal? MaxLateFee { get; set; }

        // Other Charges
        private ICollection<TenantLeaseCharge> _tenantLeaseCharges;
        public virtual ICollection<TenantLeaseCharge> TenantLeaseCharges
        {
            get { return _tenantLeaseCharges ?? (_tenantLeaseCharges = new List<TenantLeaseCharge>()); }
            protected set { _tenantLeaseCharges = value; }
        }

        private ICollection<TenantLeasePaymentSchedule> _tenantLeasePaymentSchedules;
        public virtual ICollection<TenantLeasePaymentSchedule> TenantLeasePaymentSchedules
        {
            get { return _tenantLeasePaymentSchedules ?? (_tenantLeasePaymentSchedules = new List<TenantLeasePaymentSchedule>()); }
            protected set { _tenantLeasePaymentSchedules = value; }
        }

        private ICollection<TenantPayment> _tenantPayments;
        public virtual ICollection<TenantPayment> TenantPayments
        {
            get { return _tenantPayments ?? (_tenantPayments = new List<TenantPayment>()); }
            protected set { _tenantPayments = value; }
        }
    }

    public enum TermPeriod
    {
        Months = 0,
        Weeks,
        Days
    }

    public enum DueFrequency
    {
        Monthly = 0,
        EveryTwoWeeks,
        Weekly,
        Daily,
        Quarterly,
        Every6Months,
        Yearly,
        BiMonthly,
    }

    public enum LateFeeOption
    {
        FlatFee = 0,
        BaseAmountPerDay,
        PercentOfRentPerDay
    }
}
