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
    [Validator(typeof(PurchaseOrderItemValidator))]
    public class PurchaseOrderItemModel : BaseEamEntityModel
    {
        public long? PurchaseOrderId { get; set; }

        [BaseEamResourceDisplayName("Common.Sequence")]
        [UIHint("Int32Nullable")]
        public int? Sequence { get; set; }

        [BaseEamResourceDisplayName("Item")]
        public long? ItemId { get; set; }
        public string ItemName { get; set; }
        [BaseEamResourceDisplayName("UnitOfMeasure")]
        public long? ItemUnitOfMeasureId { get; set; }
        public string ItemUnitOfMeasureName { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrderItem.QuantityOrdered")]
        [UIHint("DecimalNullable")]
        public decimal? QuantityOrdered { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrderItem.QuantityReceived")]
        [UIHint("DecimalNullable")]
        public decimal? QuantityReceived { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrderItem.ReceiveToStore")]
        public long? ReceiveToStoreId { get; set; }
        public string ReceiveToStoreName { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrderItem.ReceiveToStoreLocator")]
        public long? ReceiveToStoreLocatorId { get; set; }
        public string ReceiveToStoreLocatorName { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrderItem.UnitPrice")]
        [UIHint("DecimalNullable")]
        public decimal? UnitPrice { get; set; }

        // Tax rate percentage
        [BaseEamResourceDisplayName("PurchaseOrderItem.TaxRate")]
        [UIHint("DecimalNullable")]
        public decimal? TaxRate { get; set; }
        [BaseEamResourceDisplayName("PurchaseOrderItem.TaxAmount")]
        [UIHint("DecimalNullable")]
        public decimal? TaxAmount { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrderItem.Subtotal")]
        [UIHint("DecimalNullable")]
        public decimal? Subtotal { get; set; }
        [BaseEamResourceDisplayName("PurchaseOrderItem.SubtotalWithTax")]
        [UIHint("DecimalNullable")]
        public decimal? SubtotalWithTax { get; set; }

        //Cache SiteId from PO
        public long? SiteId { get; set; }
    }
}