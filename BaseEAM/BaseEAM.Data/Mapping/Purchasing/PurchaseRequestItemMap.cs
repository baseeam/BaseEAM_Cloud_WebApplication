/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class PurchaseRequestItemMap : BaseEamEntityTypeConfiguration<PurchaseRequestItem>
    {
        public PurchaseRequestItemMap()
        {
            this.ToTable("PurchaseRequestItem");
            this.Property(e => e.QuantityRequested).HasPrecision(19, 4);
            this.HasOptional(e => e.PurchaseRequest)
                .WithMany(e => e.PurchaseRequestItems)
                .HasForeignKey(e => e.PurchaseRequestId);
            this.HasOptional(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId);
        }
    }
}
