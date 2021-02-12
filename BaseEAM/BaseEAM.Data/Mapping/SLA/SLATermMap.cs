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
    public class SLATermMap : BaseEamEntityTypeConfiguration<SLATerm>
    {
        public SLATermMap()
        {
            this.ToTable("SLATerm");
            this.HasOptional(e => e.SLADefinition)
                .WithMany(e => e.SLATerms)
                .HasForeignKey(e => e.SLADefinitionId);
            this.Property(e => e.TrackingBaseField).HasMaxLength(64);
            this.Property(e => e.TrackingField).HasMaxLength(64);
        }
    }
}
