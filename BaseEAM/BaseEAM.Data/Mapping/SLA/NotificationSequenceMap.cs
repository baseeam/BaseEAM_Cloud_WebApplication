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
    public class NotificationSequenceMap : BaseEamEntityTypeConfiguration<NotificationSequence>
    {
        public NotificationSequenceMap()
        {
            this.ToTable("NotificationSequence");
            this.HasOptional(e => e.SLATerm)
                .WithMany(e => e.NotificationSequences)
                .HasForeignKey(e => e.SLATermId);
            this.Property(e => e.Users).HasMaxLength(512);
            this.Property(e => e.MessageTemplate).HasMaxLength(128);
        }
    }
}
