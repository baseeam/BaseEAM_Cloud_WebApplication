/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System;
using System.Collections.Generic;

namespace BaseEAM.Core.Domain
{
    public class RequestForQuotation : WorkflowBaseEntity
    {
        public long? SiteId { get; set; }
        public virtual Site Site { get; set; }

        public long? PurchaseRequestId { get; set; }
        public virtual PurchaseRequest PurchaseRequest { get; set; }

        public long? RequestorId { get; set; }
        public virtual User Requestor { get; set; }

        public DateTime? ExpectedQuoteDate { get; set; }
        public DateTime? DateRequired { get; set; }

        public long? ShipToAddressId { get; set; }
        public virtual Address ShipToAddress { get; set; }

        public bool IsSent { get; set; }

        private ICollection<RequestForQuotationItem> _requestForQuotationItems;
        public virtual ICollection<RequestForQuotationItem> RequestForQuotationItems
        {
            get { return _requestForQuotationItems ?? (_requestForQuotationItems = new List<RequestForQuotationItem>()); }
            protected set { _requestForQuotationItems = value; }
        }

        private ICollection<RequestForQuotationVendor> _requestForQuotationVendors;
        public virtual ICollection<RequestForQuotationVendor> RequestForQuotationVendors
        {
            get { return _requestForQuotationVendors ?? (_requestForQuotationVendors = new List<RequestForQuotationVendor>()); }
            protected set { _requestForQuotationVendors = value; }
        }

        public override string AssignmentType
        {
            get
            {
                return EntityType.RequestForQuotation;
            }
        }
    }
}
