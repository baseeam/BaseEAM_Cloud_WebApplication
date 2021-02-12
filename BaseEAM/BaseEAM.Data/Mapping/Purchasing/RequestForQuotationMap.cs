/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class RequestForQuotationMap : BaseEamWorkflowEntityTypeConfiguration<RequestForQuotation>
    {
        public RequestForQuotationMap()
        {
            this.ToTable("RequestForQuotation");
            this.HasOptional(e => e.Site)
                .WithMany()
                .HasForeignKey(e => e.SiteId);
            this.HasOptional(e => e.PurchaseRequest)
                .WithMany()
                .HasForeignKey(e => e.PurchaseRequestId);
            this.HasOptional(e => e.Requestor)
                .WithMany()
                .HasForeignKey(e => e.RequestorId);
            this.HasOptional(e => e.ShipToAddress)
                .WithMany()
                .HasForeignKey(e => e.ShipToAddressId);
        }
    }
}
