/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/

namespace BaseEAM.Core.Domain
{
    public class RequestForQuotationItem : BaseEntity
    {
        public long? RequestForQuotationId { get; set; }
        public virtual RequestForQuotation RequestForQuotation { get; set; }

        public int? Sequence { get; set; }

        public long? ItemId { get; set; }
        public virtual Item Item { get; set; }

        public decimal? QuantityRequested { get; set; }
    }
}
