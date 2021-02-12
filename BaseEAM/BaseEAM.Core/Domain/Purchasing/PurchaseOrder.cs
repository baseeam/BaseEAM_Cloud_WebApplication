/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System;
using System.Collections.Generic;

namespace BaseEAM.Core.Domain
{
    public class PurchaseOrder : WorkflowBaseEntity
    {
        public long? SiteId { get; set; }
        public virtual Site Site { get; set; }

        // PO generated fromt this PR
        public long? PurchaseRequestId { get; set; }
        public virtual PurchaseRequest PurchaseRequest { get; set; }

        // PO generated fromt this RFQ
        public long? RequestForQuotationId { get; set; }
        public virtual RequestForQuotation RequestForQuotation { get; set; }

        public long? RequestForQuotationVendorId { get; set; }
        public virtual RequestForQuotationVendor RequestForQuotationVendor { get; set; }

        public long? RequestorId { get; set; }
        public virtual User Requestor { get; set; }

        public long? SupervisorId { get; set; }
        public virtual User Supervisor { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? DateOrdered { get; set; }
        public DateTime? DateRequired { get; set; }

        public long? ShipToAddressId { get; set; }
        public virtual Address ShipToAddress { get; set; }

        public long? BillToAddressId { get; set; }
        public virtual Address BillToAddress { get; set; }

        public long? VendorId { get; set; }
        public virtual Company Vendor { get; set; }

        public long? PaymentTermId { get; set; }
        public virtual ValueItem PaymentTerm { get; set; }

        public long? ContractId { get; set; }
        public virtual Contract Contract { get; set; }

        public bool IsSent { get; set; }

        private ICollection<PurchaseOrderItem> _purchaseOrderItems;
        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems
        {
            get { return _purchaseOrderItems ?? (_purchaseOrderItems = new List<PurchaseOrderItem>()); }
            protected set { _purchaseOrderItems = value; }
        }

        private ICollection<PurchaseOrderMiscCost> _purchaseOrderMiscCosts;
        public virtual ICollection<PurchaseOrderMiscCost> PurchaseOrderMiscCosts
        {
            get { return _purchaseOrderMiscCosts ?? (_purchaseOrderMiscCosts = new List<PurchaseOrderMiscCost>()); }
            protected set { _purchaseOrderMiscCosts = value; }
        }

        public override decimal? AssignmentAmount
        {
            get
            {
                decimal? total = 0;
                foreach (PurchaseOrderItem item in this.PurchaseOrderItems)
                {
                    total += (item.Subtotal ?? 0);
                }
                return total;
            }
        }

        public override string AssignmentType
        {
            get
            {
                return EntityType.PurchaseOrder;
            }
        }
    }
}
