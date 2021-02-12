/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System;
using System.Collections.Generic;

namespace BaseEAM.Core.Domain
{
    public class RequestForQuotationVendor : BaseEntity
    {
        public long? RequestForQuotationId { get; set; }
        public virtual RequestForQuotation RequestForQuotation { get; set; }

        public long? VendorId { get; set; }
        public virtual Company Vendor { get; set; }

        public string VendorContactName { get; set; }
        public string VendorContactEmail { get; set; }
        public string VendorContactPhone { get; set; }
        public string VendorContactFax { get; set; }

        public string VendorQuoteNumber { get; set; }
        public DateTime? VendorQuoteDate { get; set; }

        private ICollection<RequestForQuotationVendorItem> _requestForQuotationVendorItems;
        public virtual ICollection<RequestForQuotationVendorItem> RequestForQuotationVendorItems
        {
            get { return _requestForQuotationVendorItems ?? (_requestForQuotationVendorItems = new List<RequestForQuotationVendorItem>()); }
            protected set { _requestForQuotationVendorItems = value; }
        }
    }
}
