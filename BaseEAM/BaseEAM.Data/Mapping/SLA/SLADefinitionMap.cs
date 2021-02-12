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
    public class SLADefinitionMap : BaseEamEntityTypeConfiguration<SLADefinition>
    {
        public SLADefinitionMap()
        {
            this.ToTable("SLADefinition");
            this.Property(e => e.EntityType).HasMaxLength(64);
            this.Property(e => e.Description).HasMaxLength(512);
        }
    }
}
