/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class RequestForQuotationItemMap : BaseEamEntityTypeConfiguration<RequestForQuotationItem>
    {
        public RequestForQuotationItemMap()
        {
            this.ToTable("RequestForQuotationItem");
            this.HasOptional(e => e.RequestForQuotation)
                .WithMany(e => e.RequestForQuotationItems)
                .HasForeignKey(e => e.RequestForQuotationId);
            this.HasOptional(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId);
            this.Property(e => e.QuantityRequested).HasPrecision(19, 4);
        }
    }
}
