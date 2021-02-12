/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class PurchaseRequestMap : BaseEamWorkflowEntityTypeConfiguration<PurchaseRequest>
    {
        public PurchaseRequestMap()
        {
            this.ToTable("PurchaseRequest");
            this.HasOptional(e => e.Site)
                .WithMany()
                .HasForeignKey(e => e.SiteId);
            this.HasOptional(e => e.Requestor)
                .WithMany()
                .HasForeignKey(e => e.RequestorId);
        }
    }
}
