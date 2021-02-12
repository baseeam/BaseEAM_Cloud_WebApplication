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
    public class NotificationSequenceValidator : BaseEamValidator<NotificationSequenceModel>
    {
        public NotificationSequenceValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Sequence).GreaterThan(0).WithMessage(localizationService.GetResource("Common.Sequence.Required"));
            RuleFor(x => x.Users).NotEmpty().WithMessage(localizationService.GetResource("Common.Users.Required"));
            RuleFor(x => x.MessageTemplate).NotEmpty().WithMessage(localizationService.GetResource("MessageTemplate.Required"));
            RuleFor(x => x).Must(SendingTimeSpecified).WithMessage(localizationService.GetResource("SLATerm.SendingTime.Required"));
        }

        private bool SendingTimeSpecified(NotificationSequenceModel model)
        {
            return model.SendingTimeHours > 0 || model.SendingTimeMinutes > 0;
        }
    }
}