/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/

namespace BaseEAM.Core.Domain
{
    public class RequestForQuotationVendorItem : BaseEntity
    {
        public long? RequestForQuotationVendorId { get; set; }
        public virtual RequestForQuotationVendor RequestForQuotationVendor { get; set; }

        public long? RequestForQuotationItemId { get; set; }
        public virtual RequestForQuotationItem RequestForQuotationItem { get; set; }

        public decimal? QuantityQuoted { get; set; }
        public decimal? UnitPriceQuoted { get; set; }
        public decimal? SubtotalQuoted { get; set; }

        public bool IsAwarded { get; set; }
    }
}
