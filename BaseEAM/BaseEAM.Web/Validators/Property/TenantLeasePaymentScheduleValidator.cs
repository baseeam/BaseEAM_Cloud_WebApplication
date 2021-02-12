/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Web.Framework.Validators;
using BaseEAM.Web.Models;
using FluentValidation;
using BaseEAM.Services;

namespace BaseEAM.Web.Validators
{
    public class TenantLeasePaymentScheduleValidator : BaseEamValidator<TenantLeasePaymentScheduleModel>
    {
        public TenantLeasePaymentScheduleValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.DueDate).NotEmpty().WithMessage(localizationService.GetResource("TenantLeasePaymentSchedule.DueDate.Required"));
            RuleFor(x => x.DueAmount).NotEmpty().WithMessage(localizationService.GetResource("TenantLeasePaymentSchedule.DueAmount.Required"));
        }
    }
}