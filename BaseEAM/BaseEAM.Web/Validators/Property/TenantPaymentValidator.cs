/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using Autofac;
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using BaseEAM.Services;
using BaseEAM.Web.Framework.Validators;
using BaseEAM.Web.Models;
using FluentValidation;
using System.Linq;

namespace BaseEAM.Web.Validators
{
    public class TenantPaymentValidator : BaseEamValidator<TenantPaymentModel>
    {
        private readonly IRepository<TenantPayment> _tenantPaymentRepository;
        public TenantPaymentValidator(ILocalizationService localizationService, IRepository<TenantPayment> tenantPaymentRepository)
        {
            this._tenantPaymentRepository = tenantPaymentRepository;

            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("TenantPayment.Name.Required"));
            RuleFor(x => x).Must(NoDuplication).WithMessage(localizationService.GetResource("TenantPayment.Name.Unique"));
        }

        private bool NoDuplication(TenantPaymentModel model)
        {
            var tenantPayment = _tenantPaymentRepository.GetAll().Where(c => c.Name == model.Name && c.Id != model.Id).FirstOrDefault();
            return tenantPayment == null;
        }
    }
}