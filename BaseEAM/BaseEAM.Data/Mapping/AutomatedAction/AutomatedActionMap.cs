/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;

namespace BaseEAM.Data.Mapping
{
    public class AutomatedActionMap : BaseEamEntityTypeConfiguration<AutomatedAction>
    {
        public AutomatedActionMap()
        {
            this.ToTable("AutomatedAction");
            this.Property(s => s.Description).HasMaxLength(512);
            this.Property(s => s.EntityType).HasMaxLength(64);
            this.Property(s => s.Expression).HasMaxLength(128);
            this.Property(s => s.CronExpression).HasMaxLength(64);
            this.HasOptional(e => e.ActionType)
                .WithMany()
                .HasForeignKey(e => e.ActionTypeId);
            this.Property(s => s.Users).HasMaxLength(512);
            this.Property(s => s.MessageTemplate).HasMaxLength(128);
            this.HasOptional(e => e.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(e => e.WorkflowDefinitionId);
            this.Property(s => s.SetExpression).HasMaxLength(512);
        }
    }
}
