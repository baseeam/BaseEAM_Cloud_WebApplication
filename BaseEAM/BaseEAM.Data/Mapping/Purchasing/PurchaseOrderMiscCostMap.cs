/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class PurchaseOrderMiscCostMap : BaseEamEntityTypeConfiguration<PurchaseOrderMiscCost>
    {
        public PurchaseOrderMiscCostMap()
        {
            this.ToTable("PurchaseOrderMiscCost");
            this.Property(e => e.Description).HasMaxLength(512);
            this.Property(e => e.Amount).HasPrecision(19, 4);
            this.HasOptional(e => e.PurchaseOrder)
                .WithMany(e => e.PurchaseOrderMiscCosts)
                .HasForeignKey(e => e.PurchaseOrderId);
            this.HasOptional(e => e.POMiscCostType)
                .WithMany()
                .HasForeignKey(e => e.POMiscCostTypeId);
        }
    }
}
