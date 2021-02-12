/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using BaseEAM.Services;
using BaseEAM.Web.Framework.Validators;
using BaseEAM.Web.Models;
using FluentValidation;
using System.Linq;

namespace BaseEAM.Web.Validators
{
    public class TenantLeaseChargeValidator : BaseEamValidator<TenantLeaseChargeModel>
    {
        private readonly IRepository<TenantLeaseCharge> _tenantLeaseChargeRepository;
        public TenantLeaseChargeValidator(ILocalizationService localizationService, IRepository<TenantLeaseCharge> tenantLeaseChargeRepository)
        {
            this._tenantLeaseChargeRepository = tenantLeaseChargeRepository;
            RuleFor(x => x.ChargeTypeId).NotEmpty().WithMessage(localizationService.GetResource("ChargeType.Required"));
            RuleFor(x => x.ChargeAmount).NotEmpty().WithMessage(localizationService.GetResource("TenantLeaseCharge.ChargeAmount.Required"));

            RuleFor(x => x.ValidFrom).NotEmpty().
                When(x => x.ChargeDueType == ChargeDueType.EachTimeRentIsDue || x.ChargeDueType == ChargeDueType.MonthlyOnaSpecificDay).WithMessage(localizationService.GetResource("TenantLeaseCharge.ValidFrom.Required"));
            RuleFor(x => x.ValidTo).NotEmpty().
               When(x => x.ChargeDueType == ChargeDueType.EachTimeRentIsDue || x.ChargeDueType == ChargeDueType.MonthlyOnaSpecificDay).WithMessage(localizationService.GetResource("TenantLeaseCharge.ValidTo.Required"));

            RuleFor(x => x.ChargeDueDate).NotEmpty().
               When(x => x.ChargeDueType == ChargeDueType.OnceOnlyOnaSpecificDate).WithMessage(localizationService.GetResource("TenantLeaseCharge.ChargeDueDate.Required"));

            RuleFor(x => x.ChargeDueDay).NotEmpty().
              When(x => x.ChargeDueType == ChargeDueType.MonthlyOnaSpecificDay).WithMessage(localizationService.GetResource("TenantLeaseCharge.ChargeDueDay.Required"));

            RuleFor(x => x).Must(NoDuplication).WithMessage(localizationService.GetResource("TenantLeaseCharge.ChargeType.Unique"));
        }

        private bool NoDuplication(TenantLeaseChargeModel model)
        {
            var tenantLeaseCharge = _tenantLeaseChargeRepository.GetAll().Where(c => c.ChargeTypeId == model.ChargeTypeId && c.Id != model.Id && c.TenantLeaseId == model.TenantLeaseId).FirstOrDefault();
            return tenantLeaseCharge == null;
        }
    }
}