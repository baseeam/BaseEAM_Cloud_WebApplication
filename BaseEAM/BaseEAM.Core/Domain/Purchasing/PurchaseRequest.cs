/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System;
using System.Collections.Generic;

namespace BaseEAM.Core.Domain
{
    public class PurchaseRequest : WorkflowBaseEntity
    {
        public long? SiteId { get; set; }
        public virtual Site Site { get; set; }

        public long? RequestorId { get; set; }
        public virtual User Requestor { get; set; }

        public DateTime? DateRequired { get; set; }

        private ICollection<PurchaseRequestItem> _purchaseRequestItems;
        public virtual ICollection<PurchaseRequestItem> PurchaseRequestItems
        {
            get { return _purchaseRequestItems ?? (_purchaseRequestItems = new List<PurchaseRequestItem>()); }
            protected set { _purchaseRequestItems = value; }
        }

        public override string AssignmentType
        {
            get
            {
                return EntityType.PurchaseRequest;
            }
        }
    }
}
