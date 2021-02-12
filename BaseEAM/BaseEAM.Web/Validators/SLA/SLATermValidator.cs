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
    public class SLATermValidator : BaseEamValidator<SLATermModel>
    {
        public SLATermValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.TrackingBaseField).NotEmpty().WithMessage(localizationService.GetResource("SLATerm.TrackingBaseField.Required"));
            RuleFor(x => x.TrackingField).NotEmpty().WithMessage(localizationService.GetResource("SLATerm.TrackingField.Required"));
            RuleFor(x => x).Must(LimitSpecified).WithMessage(localizationService.GetResource("SLATerm.Limit.Required"));
        }

        private bool LimitSpecified(SLATermModel model)
        {
            return model.LimitHours > 0 || model.LimitMinutes > 0;
        }
    }
}