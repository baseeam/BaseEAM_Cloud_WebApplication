/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Services;
using BaseEAM.Web.Framework.Validators;
using BaseEAM.Web.Models;
using FluentValidation;

namespace BaseEAM.Web.Validators
{
    public class PurchaseOrderMiscCostValidator : BaseEamValidator<PurchaseOrderMiscCostModel>
    {
        public PurchaseOrderMiscCostValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.POMiscCostTypeId).NotEmpty().WithMessage(localizationService.GetResource("PurchaseOrderMiscCost.POMiscCostType.Required"));
            RuleFor(x => x.Amount).NotEmpty().WithMessage(localizationService.GetResource("Common.Amount.Required"));
        }
    }
}