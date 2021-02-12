/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using BaseEAM.Services;
using BaseEAM.Web.Framework.Validators;
using BaseEAM.Web.Models;
using System.Linq;
using FluentValidation;

namespace BaseEAM.Web.Validators
{
    public class AutomatedActionValidator : BaseEamValidator<AutomatedActionModel>
    {
        private readonly IRepository<AutomatedAction> _automatedActionRepository;
        public AutomatedActionValidator(ILocalizationService localizationService, IRepository<AutomatedAction> automatedActionRepository)
        {
            this._automatedActionRepository = automatedActionRepository;

            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("AutomatedAction.Name.Required"));
            RuleFor(x => x.EntityType).NotEmpty().WithMessage(localizationService.GetResource("AutomatedAction.EntityType.Required"));
            RuleFor(x => x.ActionTypeId).NotEmpty().WithMessage(localizationService.GetResource("AutomatedAction.ActionType.Required"));
            RuleFor(x => x.HoursAfter).GreaterThan(0).When(x => x.TriggerType == ActionTriggerType.TimeBased)
                .WithMessage(localizationService.GetResource("AutomatedAction.HoursAfter.Required"));

            RuleFor(x => x.Users).NotEmpty().When(x => x.ActionTypeId == 45)
                .WithMessage(localizationService.GetResource("AutomatedAction.Users.Required"));

            RuleFor(x => x.WorkflowDefinitionId).NotEmpty().When(x => x.ActionTypeId == 43)
                .WithMessage(localizationService.GetResource("WorkflowDefinition.Required"));

            RuleFor(x => x.SetExpression).NotEmpty().When(x => x.ActionTypeId == 44)
                .WithMessage(localizationService.GetResource("AutomatedAction.SetExpression.Required"));
            RuleFor(x => x).Must(NoDuplication).WithMessage(localizationService.GetResource("AutomatedAction.Name.Unique"));
        }

        private bool NoDuplication(AutomatedActionModel model)
        {
            var automatedAction = _automatedActionRepository.GetAll().Where(c => c.Name == model.Name && c.Id != model.Id).FirstOrDefault();
            return automatedAction == null;
        }
    }
}