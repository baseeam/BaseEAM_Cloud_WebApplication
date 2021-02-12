/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Mvc;
using BaseEAM.Web.Validators;
using FluentValidation.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace BaseEAM.Web.Models
{
    [Validator(typeof(TenantPaymentValidator))]
    public class TenantPaymentModel : BaseEamEntityModel
    {
        [BaseEamResourceDisplayName("Common.Name")]
        public string Name { get; set; }

        [BaseEamResourceDisplayName("Site")]
        public long? SiteId { get; set; }
        public string SiteName { get; set; }

        [BaseEamResourceDisplayName("Tenant")]
        public long? TenantId { get; set; }
        public string TenantName { get; set; }

        [BaseEamResourceDisplayName("Property")]
        public long? PropertyId { get; set; }
        public string PropertyName { get; set; }

        [BaseEamResourceDisplayName("TenantLease")]
        public long? TenantLeaseId { get; set; }
        public string TenantLeaseNumber { get; set; }

        [BaseEamResourceDisplayName("TenantLeasePaymentSchedule")]
        public long? TenantLeasePaymentScheduleId { get; set; }

        [BaseEamResourceDisplayName("TenantLeaseCharge")]
        public long? TenantLeaseChargeId { get; set; }

        [BaseEamResourceDisplayName("TenantPayment.DueDate")]
        [UIHint("DateNullable")]
        public DateTime? DueDate { get; set; }

        [BaseEamResourceDisplayName("ChargeType")]
        public long? ChargeTypeId { get; set; }
        public string ChargeTypeName { get; set;}

        [BaseEamResourceDisplayName("TenantPayment.DueAmount")]
        [UIHint("DecimalNullable")]
        public decimal? DueAmount { get; set; }

        [BaseEamResourceDisplayName("TenantPayment.DaysLateCount")]
        public int DaysLateCount { get; set; }

        [BaseEamResourceDisplayName("TenantPayment.LateFeeAmount")]
        [UIHint("DecimalNullable")]
        public decimal? LateFeeAmount { get; set; }

        [BaseEamResourceDisplayName("TenantPayment.CollectedAmount")]
        [UIHint("DecimalNullable")]
        public decimal? CollectedAmount { get; set; }

        [BaseEamResourceDisplayName("TenantPayment.BalanceAmount")]
        [UIHint("DecimalNullable")]
        public decimal? BalanceAmount { get; set; }
    }
}