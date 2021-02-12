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
    [Validator(typeof(RequestForQuotationItemValidator))]
    public class RequestForQuotationItemModel : BaseEamEntityModel
    {
        public long? RequestForQuotationId { get; set; }

        [BaseEamResourceDisplayName("Common.Sequence")]
        [UIHint("Int32Nullable")]
        public int? Sequence { get; set; }

        [BaseEamResourceDisplayName("Item")]
        public long? ItemId { get; set; }
        public string ItemName { get; set; }
        [BaseEamResourceDisplayName("UnitOfMeasure")]
        public long? ItemUnitOfMeasureId { get; set; }
        public string ItemUnitOfMeasureName { get; set; }

        [BaseEamResourceDisplayName("RequestForQuotationItem.QuantityRequested")]
        [UIHint("DecimalNullable")]
        public decimal? QuantityRequested { get; set; }
    }
}