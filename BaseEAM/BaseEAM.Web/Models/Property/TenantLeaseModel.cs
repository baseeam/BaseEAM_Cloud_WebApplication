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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BaseEAM.Web.Models
{
    [Validator(typeof(TenantLeaseValidator))]
    public class TenantLeaseModel : BaseEamEntityModel
    {
        public TenantLeaseModel()
        {
            AvailableTermNumbers = new List<SelectListItem>();
            AvailableBiMonthlyStarts = new List<SelectListItem>();
            AvailableBiMonthlyEnds = new List<SelectListItem>();
        }

        [BaseEamResourceDisplayName("Common.Number")]
        public string Number { get; set; }

        [BaseEamResourceDisplayName("Common.Description")]
        public string Description { get; set; }

        [BaseEamResourceDisplayName("Common.Priority")]
        public AssignmentPriority Priority { get; set; }
        public string PriorityText { get; set; }

        [BaseEamResourceDisplayName("Common.Status")]
        public string Status { get; set; }

        [BaseEamResourceDisplayName("Site")]
        public long? SiteId { get; set; }
        public string SiteName { get; set; }

        [BaseEamResourceDisplayName("Tenant")]
        public long? TenantId { get; set; }
        public string TenantName { get; set; }

        [BaseEamResourceDisplayName("Property")]
        public long? PropertyId { get; set; }
        public string PropertyName { get; set; }

        // Term
        [BaseEamResourceDisplayName("TenantLease.TermStartDate")]
        [UIHint("DateNullable")]
        public DateTime? TermStartDate { get; set; }

        [BaseEamResourceDisplayName("TenantLease.TermEndDate")]
        [UIHint("DateNullable")]
        public DateTime? TermEndDate { get; set; }

        [BaseEamResourceDisplayName("TenantLease.TermNumber")]
        [UIHint("Int32Nullable")]
        public int? TermNumber { get; set; }

        [BaseEamResourceDisplayName("TenantLease.TermPeriod")]
        public TermPeriod TermPeriod { get; set; }

        [BaseEamResourceDisplayName("TenantLease.TermIsMonthToMonth")]
        public bool TermIsMonthToMonth { get; set; }

        [BaseEamResourceDisplayName("TenantLease.TermRentAmount")]
        [UIHint("DecimalNullable")]
        public decimal? TermRentAmount { get; set; }

        [BaseEamResourceDisplayName("TenantLease.DueFrequency")]
        public DueFrequency DueFrequency { get; set; }

        [BaseEamResourceDisplayName("TenantLease.BiMonthlyStart")]
        [UIHint("Int32Nullable")]
        public int? BiMonthlyStart { get; set; }

        [BaseEamResourceDisplayName("TenantLease.BiMonthlyEnd")]
        [UIHint("Int32Nullable")]
        public int? BiMonthlyEnd { get; set; }


        [BaseEamResourceDisplayName("TenantLease.FirstPaymentDate")]
        [UIHint("DateNullable")]
        public DateTime? FirstPaymentDate { get; set; }

        // Late Fee
        [BaseEamResourceDisplayName("TenantLease.LateFeeEnabled")]
        public bool LateFeeEnabled { get; set; }

        [BaseEamResourceDisplayName("TenantLease.GracePeriodDays")]
        [UIHint("Int32Nullable")]
        public int? GracePeriodDays { get; set; }

        [BaseEamResourceDisplayName("TenantLease.LateFeeOption")]
        [UIHint("Int32Nullable")]
        public LateFeeOption LateFeeOption { get; set; }

        [BaseEamResourceDisplayName("TenantLease.FlatFee")]
        [UIHint("DecimalNullable")]
        public decimal? FlatFee { get; set; }

        [BaseEamResourceDisplayName("TenantLease.BaseAmountPerDay")]
        [UIHint("DecimalNullable")]
        public decimal? BaseAmountPerDay { get; set; }

        [BaseEamResourceDisplayName("TenantLease.AmountPerDay")]
        [UIHint("DecimalNullable")]
        public decimal? AmountPerDay { get; set; }

        [BaseEamResourceDisplayName("TenantLease.PercentOfRentPerDay")]
        [UIHint("DecimalNullable")]
        public decimal? PercentOfRentPerDay { get; set; }

        public bool StopPerDay { get; set; }

        [BaseEamResourceDisplayName("TenantLease.MaxLateFee")]
        [UIHint("DecimalNullable")]
        public decimal? MaxLateFee { get; set; }

        public List<SelectListItem> AvailableTermNumbers { get; set; }
        public List<SelectListItem> AvailableBiMonthlyStarts { get; set; }
        public List<SelectListItem> AvailableBiMonthlyEnds { get; set; }
        /// <summary>
        /// Cache available actions from assignment
        /// </summary>
        public string AvailableActions { get; set; }

        [BaseEamResourceDisplayName("Common.AssignedUsers")]
        public string AssignedUsers { get; set; }

        public string Comment { get; set; }
        public string ActionName { get; set; }
    }
}