/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class TenantMap : BaseEamEntityTypeConfiguration<Tenant>
    {
        public TenantMap()
            : base()
        {
            this.ToTable("Tenant");
            this.HasOptional(e => e.Address)
                .WithMany()
                .HasForeignKey(e => e.AddressId);
            this.HasOptional(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
        }
    }
}
