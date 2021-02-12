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
    [Validator(typeof(TenantLeaseChargeValidator))]
    public class TenantLeaseChargeModel : BaseEamEntityModel
    {
        public TenantLeaseChargeModel()
        {
            AvailableChargeDueDays = new List<SelectListItem>();
        }

        public long? TenantLeaseId { get; set; }

        [BaseEamResourceDisplayName("ChargeType")]
        public long? ChargeTypeId { get; set; }
        public string ChargeTypeName { get; set; }

        [BaseEamResourceDisplayName("TenantLeaseCharge.ChargeAmount")]
        [UIHint("DecimalNullable")]
        public decimal? ChargeAmount { get; set; }

        [BaseEamResourceDisplayName("TenantLeaseCharge.AmountIsOverridable")]
        public bool AmountIsOverridable { get; set; }

        [BaseEamResourceDisplayName("TenantLeaseCharge.ChargeDueType")]
        public ChargeDueType ChargeDueType { get; set; }
        public string ChargeDueTypeText { get; set; }

        [BaseEamResourceDisplayName("TenantLeaseCharge.ChargeDueDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? ChargeDueDate { get; set; }

        [BaseEamResourceDisplayName("TenantLeaseCharge.ChargeDueDay")]
        public int? ChargeDueDay { get; set; }

        [BaseEamResourceDisplayName("TenantLeaseCharge.ValidFrom")]
        [UIHint("DateTimeNullable")]
        public DateTime? ValidFrom { get; set; }

        [BaseEamResourceDisplayName("TenantLeaseCharge.ValidTo")]
        [UIHint("DateTimeNullable")]
        public DateTime? ValidTo { get; set; }

        public List<SelectListItem> AvailableChargeDueDays { get; set; }
    }
}