/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class PurchaseOrderMap : BaseEamWorkflowEntityTypeConfiguration<PurchaseOrder>
    {
        public PurchaseOrderMap()
        {
            this.ToTable("PurchaseOrder");
            this.HasOptional(e => e.Site)
                .WithMany()
                .HasForeignKey(e => e.SiteId);
            this.HasOptional(e => e.PurchaseRequest)
                .WithMany()
                .HasForeignKey(e => e.PurchaseRequestId);
            this.HasOptional(e => e.RequestForQuotation)
                .WithMany()
                .HasForeignKey(e => e.RequestForQuotationId);
            this.HasOptional(e => e.RequestForQuotationVendor)
                .WithMany()
                .HasForeignKey(e => e.RequestForQuotationVendorId);
            this.HasOptional(e => e.Requestor)
                .WithMany()
                .HasForeignKey(e => e.RequestorId);
            this.HasOptional(e => e.Supervisor)
                .WithMany()
                .HasForeignKey(e => e.SupervisorId);
            this.HasOptional(e => e.ShipToAddress)
                .WithMany()
                .HasForeignKey(e => e.ShipToAddressId);
            this.HasOptional(e => e.BillToAddress)
                .WithMany()
                .HasForeignKey(e => e.BillToAddressId);
            this.HasOptional(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorId);
            this.HasOptional(e => e.PaymentTerm)
                .WithMany()
                .HasForeignKey(e => e.PaymentTermId);
            this.HasOptional(e => e.Contract)
                .WithMany()
                .HasForeignKey(e => e.ContractId);
        }
    }
}
