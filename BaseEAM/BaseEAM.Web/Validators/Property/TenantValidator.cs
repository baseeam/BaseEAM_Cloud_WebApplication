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
    public class TenantValidator : BaseEamValidator<TenantModel>
    {
        private readonly IRepository<Tenant> _tenantRepository;
        public TenantValidator(ILocalizationService localizationService, IRepository<Tenant> tenantRepository)
        {
            this._tenantRepository = tenantRepository;

            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Tenant.Name.Required"));
            RuleFor(x => x).Must(NoDuplication).WithMessage(localizationService.GetResource("Tenant.Name.Unique"));
        }

        private bool NoDuplication(TenantModel model)
        {
            var tenant = _tenantRepository.GetAll().Where(c => c.Name == model.Name && c.Id != model.Id).FirstOrDefault();
            return tenant == null;
        }
    }
}