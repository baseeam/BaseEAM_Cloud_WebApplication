/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Mvc;
using BaseEAM.Web.Validators;
using FluentValidation.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BaseEAM.Web.Models
{
    [Validator(typeof(PurchaseOrderMiscCostValidator))]
    public class PurchaseOrderMiscCostModel : BaseEamEntityModel
    {
        public long? PurchaseOrderId { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrderMiscCost.POMiscCostType")]
        public long? POMiscCostTypeId { get; set; }
        public string POMiscCostTypeName { get; set; }

        [BaseEamResourceDisplayName("Common.Description")]
        public string Description { get; set; }

        [BaseEamResourceDisplayName("Common.Amount")]
        [UIHint("DecimalNullable")]
        public decimal? Amount { get; set; }
    }
}