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
    [Validator(typeof(RequestForQuotationVendorItemValidator))]
    public class RequestForQuotationVendorItemModel : BaseEamEntityModel
    {
        public long? RequestForQuotationVendorId { get; set; }

        public long? RequestForQuotationItemId { get; set; }

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

        [BaseEamResourceDisplayName("RequestForQuotationVendorItem.QuantityQuoted")]
        [UIHint("DecimalNullable")]
        public decimal? QuantityQuoted { get; set; }

        [BaseEamResourceDisplayName("RequestForQuotationVendorItem.UnitPriceQuoted")]
        [UIHint("DecimalNullable")]
        public decimal? UnitPriceQuoted { get; set; }

        [BaseEamResourceDisplayName("RequestForQuotationVendorItem.SubtotalQuoted")]
        [UIHint("DecimalNullable")]
        public decimal? SubtotalQuoted { get; set; }

        [BaseEamResourceDisplayName("RequestForQuotationVendorItem.IsAwarded")]
        [UIHint("DecimalNullable")]
        public bool IsAwarded { get; set; }
    }
}