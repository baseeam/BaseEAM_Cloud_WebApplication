/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class RequestForQuotationVendorMap : BaseEamEntityTypeConfiguration<RequestForQuotationVendor>
    {
        public RequestForQuotationVendorMap()
        {
            this.ToTable("RequestForQuotationVendor");
            this.HasOptional(e => e.RequestForQuotation)
                .WithMany(e => e.RequestForQuotationVendors)
                .HasForeignKey(e => e.RequestForQuotationId);
            this.HasOptional(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorId);
            this.Property(e => e.VendorContactName).HasMaxLength(64);
            this.Property(e => e.VendorContactEmail).HasMaxLength(64);
            this.Property(e => e.VendorContactPhone).HasMaxLength(64);
            this.Property(e => e.VendorContactFax).HasMaxLength(64);
            this.Property(e => e.VendorQuoteNumber).HasMaxLength(64);
        }
    }
}
