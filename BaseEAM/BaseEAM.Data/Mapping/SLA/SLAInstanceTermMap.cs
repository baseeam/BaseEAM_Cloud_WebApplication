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
    public class SLAInstanceTermMap : BaseEamEntityTypeConfiguration<SLAInstanceTerm>
    {
        public SLAInstanceTermMap()
        {
            this.ToTable("SLAInstanceTerm");
            this.HasOptional(e => e.SLAInstance)
                .WithMany(e => e.SLAInstanceTerms)
                .HasForeignKey(e => e.SLAInstanceId);
            this.HasOptional(e => e.SLATerm)
                .WithMany()
                .HasForeignKey(e => e.SLATermId);
        }
    }
}
