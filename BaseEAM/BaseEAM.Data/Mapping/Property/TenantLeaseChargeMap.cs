/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class TenantLeaseChargeMap : BaseEamEntityTypeConfiguration<TenantLeaseCharge>
    {
        public TenantLeaseChargeMap()
            : base()
        {
            this.ToTable("TenantLeaseCharge");
            this.HasOptional(e => e.TenantLease)
                .WithMany(e => e.TenantLeaseCharges)
                .HasForeignKey(e => e.TenantLeaseId);
            this.HasOptional(e => e.ChargeType)
                .WithMany()
                .HasForeignKey(e => e.ChargeTypeId);
            this.Property(e => e.ChargeAmount).HasPrecision(19, 4);
        }
    }
}
