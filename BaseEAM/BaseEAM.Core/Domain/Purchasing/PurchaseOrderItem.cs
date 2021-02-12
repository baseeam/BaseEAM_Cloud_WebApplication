/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
namespace BaseEAM.Core.Domain
{
    public class PurchaseOrderItem : BaseEntity
    {
        public long? PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }        

        public int? Sequence { get; set; }

        public long? ItemId { get; set; }
        public virtual Item Item { get; set; }

        public decimal? QuantityOrdered { get; set; }
        public decimal? QuantityReceived { get; set; }
        public decimal? UnitPrice { get; set; }

        public long? ReceiveToStoreId { get; set; }
        public virtual Store ReceiveToStore { get; set; }

        public long? ReceiveToStoreLocatorId { get; set; }
        public virtual StoreLocator ReceiveToStoreLocator { get; set; }

        // Tax rate percentage
        public decimal? TaxRate { get; set; }
        public decimal? TaxAmount { get; set; }

        public decimal? Subtotal { get; set; }
        public decimal? SubtotalWithTax { get; set; }
    }
}
