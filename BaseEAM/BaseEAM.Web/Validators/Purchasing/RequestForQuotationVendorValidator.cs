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
    public class RequestForQuotationVendorValidator : BaseEamValidator<RequestForQuotationVendorModel>
    {
        public RequestForQuotationVendorValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.VendorContactName).NotEmpty().WithMessage(localizationService.GetResource("VendorContactName.Required"));
            RuleFor(x => x.VendorContactEmail).NotEmpty().WithMessage(localizationService.GetResource("VendorContactEmail.Required"));
            RuleFor(x => x.VendorContactPhone).NotEmpty().WithMessage(localizationService.GetResource("VendorContactPhone.Required"));
        }
    }
}