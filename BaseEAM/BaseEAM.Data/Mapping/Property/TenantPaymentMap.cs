/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class TenantPaymentMap : BaseEamEntityTypeConfiguration<TenantPayment>
    {
        public TenantPaymentMap()
        {
            this.ToTable("TenantPayment");
            this.HasOptional(e => e.Site)
                .WithMany()
                .HasForeignKey(e => e.SiteId);
            this.HasOptional(e => e.Tenant)
                .WithMany(e => e.TenantPayments)
                .HasForeignKey(e => e.TenantId);
            this.HasOptional(e => e.Property)
                .WithMany()
                .HasForeignKey(e => e.PropertyId);
            this.HasOptional(e => e.TenantLease)
                .WithMany(e => e.TenantPayments)
                .HasForeignKey(e => e.TenantLeaseId);
            this.HasOptional(e => e.ChargeType)
                .WithMany()
                .HasForeignKey(e => e.ChargeTypeId);
            this.HasOptional(e => e.TenantLeasePaymentSchedule)
                .WithMany()
                .HasForeignKey(e => e.TenantLeasePaymentScheduleId);
            this.HasOptional(e => e.TenantLeaseCharge)
                .WithMany()
                .HasForeignKey(e => e.TenantLeaseChargeId);
            this.Property(e => e.DueAmount).HasPrecision(19, 4);
            this.Property(e => e.LateFeeAmount).HasPrecision(19, 4);
            this.Property(e => e.CollectedAmount).HasPrecision(19, 4);
            this.Property(e => e.BalanceAmount).HasPrecision(19, 4);
        }
    }
}
