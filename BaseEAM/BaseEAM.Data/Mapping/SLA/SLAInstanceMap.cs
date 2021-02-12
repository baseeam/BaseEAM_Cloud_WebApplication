/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseEAM.Data.Mapping
{
    public class SLAInstanceMap : BaseEamEntityTypeConfiguration<SLAInstance>
    {
        public SLAInstanceMap()
        {
            this.ToTable("SLAInstance");
            this.HasOptional(e => e.SLADefinition)
                .WithMany()
                .HasForeignKey(e => e.SLADefinitionId);
            this.Property(e => e.EntityType).HasMaxLength(64);
        }
    }
}
