/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;
using BaseEAM.Services;
using BaseEAM.Web.Framework.Validators;
using BaseEAM.Web.Models;
using FluentValidation;

namespace BaseEAM.Web.Validators
{
    public class TenantLeaseValidator : BaseEamValidator<TenantLeaseModel>
    {

        public TenantLeaseValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.SiteId).NotEmpty().WithMessage(localizationService.GetResource("Site.Required"));
            RuleFor(x => x.TenantId).NotEmpty().WithMessage(localizationService.GetResource("Tenant.Required"));
            RuleFor(x => x.PropertyId).NotEmpty().WithMessage(localizationService.GetResource("Property.Required"));

            RuleFor(x => x.TermStartDate).NotEmpty()
                .When(x => x.IsNew == false).WithMessage(localizationService.GetResource("TenantLease.TermStartDate.Required"));
            RuleFor(x => x.TermEndDate).NotEmpty()
                .When(x => x.TermIsMonthToMonth == false && x.IsNew == false).WithMessage(localizationService.GetResource("TenantLease.TermEndDate.Required"));
            RuleFor(x => x.TermRentAmount).NotEmpty()
                .When(x => x.IsNew == false).WithMessage(localizationService.GetResource("TenantLease.TermRentAmount.Required"));
            RuleFor(x => x.FirstPaymentDate).NotEmpty()
                .When(x => x.IsNew == false).WithMessage(localizationService.GetResource("TenantLease.FirstPaymentDate.Required"));
            RuleFor(x => x).Must(HasFirstPaymentDateGreaterThanOrEqualTermStartDate).WithMessage(localizationService.GetResource("TenantLease.FirstPaymentDateGreaterThanOrEqualTermStartDate"));
            RuleFor(x => x).Must(HasTermEndDateGreaterThanOrEqualTermStartDate).WithMessage(localizationService.GetResource("TenantLease.TermEndDateGreaterThanOrEqualTermStartDate"));
            //Late Fee
            RuleFor(x => x.GracePeriodDays).NotEmpty().
                When(x => x.LateFeeEnabled).WithMessage(localizationService.GetResource("TenantLease.GracePeriodDays.Required"));
            RuleFor(x => x.FlatFee).NotEmpty().
                When(x => x.LateFeeEnabled && x.LateFeeOption == LateFeeOption.FlatFee).WithMessage(localizationService.GetResource("TenantLease.FlatFee.Required"));
            RuleFor(x => x.BaseAmountPerDay).NotEmpty().
                When(x => x.LateFeeEnabled && x.LateFeeOption == LateFeeOption.BaseAmountPerDay).WithMessage(localizationService.GetResource("TenantLease.BaseAmountPerDay.Required"));
            RuleFor(x => x.AmountPerDay).NotEmpty().
                When(x => x.LateFeeEnabled && x.LateFeeOption == LateFeeOption.BaseAmountPerDay).WithMessage(localizationService.GetResource("TenantLease.AmountPerDay.Required"));
            RuleFor(x => x.PercentOfRentPerDay).NotEmpty().
                When(x => x.LateFeeEnabled && x.LateFeeOption == LateFeeOption.PercentOfRentPerDay).WithMessage(localizationService.GetResource("TenantLease.PercentOfRentPerDay.Required"));
        }

        private bool HasFirstPaymentDateGreaterThanOrEqualTermStartDate(TenantLeaseModel model)
        {
            return model.IsNew == true || model.FirstPaymentDate >= model.TermStartDate;
        }

        private bool HasTermEndDateGreaterThanOrEqualTermStartDate(TenantLeaseModel model)
        {
            return model.IsNew == true || model.TermEndDate >= model.TermStartDate;
        }
    }
}