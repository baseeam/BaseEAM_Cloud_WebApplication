/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Mvc;
using BaseEAM.Web.Validators;
using FluentValidation.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace BaseEAM.Web.Models
{
    [Validator(typeof(TenantLeasePaymentScheduleValidator))]
    public class TenantLeasePaymentScheduleModel : BaseEamEntityModel
    {
        public long? TenantLeaseId { get; set; }

        [BaseEamResourceDisplayName("TenantLeasePaymentSchedule.DueAmount")]
        public decimal? DueAmount { get; set; }

        [BaseEamResourceDisplayName("TenantLeasePaymentSchedule.DueDate")]
        [UIHint("DateNullable")]
        public DateTime? DueDate { get; set; }

    }
}