/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Mvc;
using BaseEAM.Web.Validators;
using FluentValidation.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BaseEAM.Web.Models
{
    [Validator(typeof(AutomatedActionValidator))]
    public class AutomatedActionModel : BaseEamEntityModel
    {
        [BaseEamResourceDisplayName("Common.Name")]
        public string Name { get; set; }

        [BaseEamResourceDisplayName("Common.Description")]
        public string Description { get; set; }

        [BaseEamResourceDisplayName("EntityType")]
        public string EntityType { get; set; }

        [BaseEamResourceDisplayName("AutomatedAction.WhenUsed")]
        public ActionWhenUsed WhenUsed { get; set; }
        public string WhenUsedText { get; set; }

        [BaseEamResourceDisplayName("AutomatedAction.Expression")]
        public string Expression { get; set; }

        [BaseEamResourceDisplayName("AutomatedAction.TriggerType")]
        public ActionTriggerType TriggerType { get; set; }
        public string TriggerTypeText { get; set; }

        [BaseEamResourceDisplayName("AutomatedAction.HoursAfter")]
        public int HoursAfter { get; set; }

        [BaseEamResourceDisplayName("AutomatedAction.ActionType")]
        public long? ActionTypeId { get; set; }
        public string ActionTypeName { get; set; }

        // Send Message
        [BaseEamResourceDisplayName("AutomatedAction.Users")]
        public string Users { get; set; }

        [BaseEamResourceDisplayName("MessageTemplate")]
        public string MessageTemplate { get; set; }

        // Launch WF
        [BaseEamResourceDisplayName("WorkflowDefinition")]
        public long? WorkflowDefinitionId { get; set; }

        // Set Value
        [BaseEamResourceDisplayName("AutomatedAction.SetExpression")]
        public string SetExpression { get; set; }
    }
}