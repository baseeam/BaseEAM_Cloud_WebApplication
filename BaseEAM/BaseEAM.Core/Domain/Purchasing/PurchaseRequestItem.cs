/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
namespace BaseEAM.Core.Domain
{
    public class PurchaseRequestItem : BaseEntity
    {
        public long? PurchaseRequestId { get; set; }
        public virtual PurchaseRequest PurchaseRequest { get; set; }

        public int? Sequence { get; set; }

        public long? ItemId { get; set; }
        public virtual Item Item { get; set; }

        // Quantity is in default UOM
        public decimal? QuantityRequested { get; set; }
    }
}
