/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class TenantPaymentCollectionMap : BaseEamEntityTypeConfiguration<TenantPaymentCollection>
    {
        public TenantPaymentCollectionMap()
        {
            this.ToTable("TenantPaymentCollection");
            this.HasOptional(e => e.TenantPayment)
                .WithMany(e => e.TenantPaymentCollections)
                .HasForeignKey(e => e.TenantPaymentId);
            this.Property(e => e.ReceivedAmount).HasPrecision(19, 4);
            this.Property(e => e.CheckNum).HasMaxLength(128);
        }
    }
}
