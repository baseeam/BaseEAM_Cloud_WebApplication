/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BaseEAM.Core.Domain
{
    public class AutomatedAction : BaseEntity
    {
        public string Description { get; set; }

        public string EntityType { get; set; }

        public int? WhenUsed { get; set; }

        public string Expression { get; set; }

        public int? TriggerType { get; set; }

        public int HoursAfter { get; set; }

        public int RepeatCount { get; set; }
        public int RepeatInterval { get; set; }

        public string CronExpression { get; set; }

        public long? ActionTypeId { get; set; }
        public virtual ValueItem ActionType { get; set; }

        // Send Message
        public string Users { get; set; }
        public string MessageTemplate { get; set; }

        // Launch WF
        public long? WorkflowDefinitionId { get; set; }
        public virtual WorkflowDefinition WorkflowDefinition { get; set; }

        // Set Value
        public string SetExpression { get; set; }
    }

    /// <summary>
    /// All actions can be detected by 'Modified' action
    /// in AuditTrail
    /// </summary>
    public enum ActionWhenUsed
    {
        /// <summary>
        /// This happens when IsNew change from true to false
        /// There's a drawback if we insert an entity with IsNew = false,
        /// It's not happen if we insert new entity on UI, because the UI
        /// follow the pattern: Create (IsNew = true) then Save (IsNew = false)
        /// </summary>
        [Description("Event.EntityInserted.Hint")]
        [Display(Name = "Event.EntityInserted")]
        EntityInserted,

        [Description("Event.EntityUpdated.Hint")]
        [Display(Name = "Event.EntityUpdated")]
        EntityUpdated,

        /// <summary>
        /// This happens when IsDeleted change from false to true
        /// </summary>
        [Description("Event.EntityDeleted.Hint")]
        [Display(Name = "Event.EntityDeleted")]
        EntityDeleted
    }

    public enum ActionTriggerType
    {
        Immediately = 0,
        TimeBased,
        Cron
    }
}
