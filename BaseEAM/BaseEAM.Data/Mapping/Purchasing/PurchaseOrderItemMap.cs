/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class PurchaseOrderItemMap : BaseEamEntityTypeConfiguration<PurchaseOrderItem>
    {
        public PurchaseOrderItemMap()
        {
            this.ToTable("PurchaseOrderItem");
            this.HasOptional(e => e.PurchaseOrder)
                .WithMany(e => e.PurchaseOrderItems)
                .HasForeignKey(e => e.PurchaseOrderId);
            this.HasOptional(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId);
            this.HasOptional(e => e.ReceiveToStore)
                .WithMany()
                .HasForeignKey(e => e.ReceiveToStoreId);
            this.HasOptional(e => e.ReceiveToStoreLocator)
                .WithMany()
                .HasForeignKey(e => e.ReceiveToStoreLocatorId);
            this.Property(e => e.QuantityOrdered).HasPrecision(19, 4);
            this.Property(e => e.QuantityReceived).HasPrecision(19, 4);
            this.Property(e => e.UnitPrice).HasPrecision(19, 4);
            this.Property(e => e.TaxRate).HasPrecision(19, 4);
            this.Property(e => e.TaxAmount).HasPrecision(19, 4);
            this.Property(e => e.Subtotal).HasPrecision(19, 4);
            this.Property(e => e.SubtotalWithTax).HasPrecision(19, 4);
        }
    }
}
