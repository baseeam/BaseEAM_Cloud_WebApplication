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
    public class TenantPaymentCollectionValidator : BaseEamValidator<TenantPaymentCollectionModel>
    {
        private readonly IRepository<TenantPaymentCollection> _tenantPaymentCollectionRepository;
        public TenantPaymentCollectionValidator(ILocalizationService localizationService, IRepository<TenantPaymentCollection> tenantPaymentCollectionRepository)
        {
            this._tenantPaymentCollectionRepository = tenantPaymentCollectionRepository;

            RuleFor(x => x.ReceivedDate).NotEmpty().WithMessage(localizationService.GetResource("TenantPaymentCollection.ReceivedDate.Required"));
            RuleFor(x => x.ReceivedAmount).NotEmpty().WithMessage(localizationService.GetResource("TenantPaymentCollection.ReceivedAmount.Required"));
            RuleFor(x => x.PaymentMethodId).NotEmpty().WithMessage(localizationService.GetResource("TenantPaymentCollection.PaymentMethod.Required"));
        }
    }
}