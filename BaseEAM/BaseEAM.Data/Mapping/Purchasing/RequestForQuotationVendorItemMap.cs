/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class RequestForQuotationVendorItemMap : BaseEamEntityTypeConfiguration<RequestForQuotationVendorItem>
    {
        public RequestForQuotationVendorItemMap()
        {
            this.ToTable("RequestForQuotationVendorItem");
            this.HasOptional(e => e.RequestForQuotationVendor)
                .WithMany(e => e.RequestForQuotationVendorItems)
                .HasForeignKey(e => e.RequestForQuotationVendorId);
            this.HasOptional(e => e.RequestForQuotationItem)
                .WithMany()
                .HasForeignKey(e => e.RequestForQuotationItemId);
            this.Property(e => e.QuantityQuoted).HasPrecision(19, 4);
            this.Property(e => e.UnitPriceQuoted).HasPrecision(19, 4);
            this.Property(e => e.SubtotalQuoted).HasPrecision(19, 4);
        }
    }
}
