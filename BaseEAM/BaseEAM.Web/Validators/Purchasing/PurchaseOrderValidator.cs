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
    public class PurchaseOrderValidator : BaseEamValidator<PurchaseOrderModel>
    {
        public PurchaseOrderValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.SiteId).NotEmpty().WithMessage(localizationService.GetResource("Site.Required"));
            RuleFor(x => x.VendorId).NotEmpty().WithMessage(localizationService.GetResource("Vendor.Required"));

            RuleFor(x => x.ShipToAddressName).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Common.Name.Required"));
            RuleFor(x => x.ShipToAddressCountry).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Address.Country.Required"));
            RuleFor(x => x.ShipToAddressCity).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Address.City.Required"));
            RuleFor(x => x.ShipToAddressAddress1).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Address.Address1.Required"));
            RuleFor(x => x.ShipToAddressZipPostalCode).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Address.ZipPostalCode.Required"));
            RuleFor(x => x.ShipToAddressPhoneNumber).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Address.PhoneNumber.Required"));
            RuleFor(x => x.ShipToAddressEmail).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Address.Email.Required"));

            RuleFor(x => x.BillToAddressName).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Common.Name.Required"));
            RuleFor(x => x.BillToAddressCountry).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Address.Country.Required"));
            RuleFor(x => x.BillToAddressCity).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Address.City.Required"));
            RuleFor(x => x.BillToAddressAddress1).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Address.Address1.Required"));
            RuleFor(x => x.BillToAddressZipPostalCode).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Address.ZipPostalCode.Required"));
            RuleFor(x => x.BillToAddressPhoneNumber).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Address.PhoneNumber.Required"));
            RuleFor(x => x.BillToAddressEmail).NotEmpty().When(x => x.IsNew == false).WithMessage(localizationService.GetResource("Address.Email.Required"));
        }
    }
}