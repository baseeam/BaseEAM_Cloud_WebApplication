/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class TenantLeaseMap : BaseEamWorkflowEntityTypeConfiguration<TenantLease>
    {
        public TenantLeaseMap()
        {
            this.ToTable("TenantLease");
            this.HasOptional(e => e.Site)
                .WithMany()
                .HasForeignKey(e => e.SiteId);
            this.HasOptional(e => e.Tenant)
                .WithMany(e => e.TenantLeases)
                .HasForeignKey(e => e.TenantId);
            this.HasOptional(e => e.Property)
                .WithMany(e => e.TenantLeases)
                .HasForeignKey(e => e.PropertyId);
            this.Property(e => e.TermRentAmount).HasPrecision(19, 4);
            this.Property(e => e.FlatFee).HasPrecision(19, 4);
            this.Property(e => e.BaseAmountPerDay).HasPrecision(19, 4);
            this.Property(e => e.AmountPerDay).HasPrecision(19, 4);
            this.Property(e => e.PercentOfRentPerDay).HasPrecision(19, 4);
            this.Property(e => e.MaxLateFee).HasPrecision(19, 4);
        }
    }
}
