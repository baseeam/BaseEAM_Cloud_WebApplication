/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class PropertyMap : BaseEamEntityTypeConfiguration<Property>
    {
        public PropertyMap()
            : base()
        {
            this.ToTable("Property");
            this.HasOptional(e => e.Site)
                .WithMany()
                .HasForeignKey(e => e.SiteId);
            this.HasOptional(e => e.Location)
                .WithMany()
                .HasForeignKey(e => e.LocationId);
        }
    }
}
