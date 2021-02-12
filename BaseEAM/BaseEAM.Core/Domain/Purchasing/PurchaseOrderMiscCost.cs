/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/

namespace BaseEAM.Core.Domain
{
    public class PurchaseOrderMiscCost : BaseEntity
    {
        public long? PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }

        public long? POMiscCostTypeId { get; set; }
        public virtual ValueItem POMiscCostType { get; set; }

        public string Description { get; set; }

        public decimal? Amount { get; set; }
    }
}
