/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class TenantLeasePaymentScheduleMap : BaseEamEntityTypeConfiguration<TenantLeasePaymentSchedule>
    {
        public TenantLeasePaymentScheduleMap()
            : base()
        {
            this.ToTable("TenantLeasePaymentSchedule");
            this.HasOptional(e => e.TenantLease)
                .WithMany(e => e.TenantLeasePaymentSchedules)
                .HasForeignKey(e => e.TenantLeaseId);
            this.Property(e => e.DueAmount).HasPrecision(19, 4);
        }
    }
}
