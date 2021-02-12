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
    public class RequestForQuotationVendorItemValidator : BaseEamValidator<RequestForQuotationVendorItemModel>
    {
        private readonly IRepository<RequestForQuotationVendor> _requestForQuotationVendorRepository;
        private readonly IRepository<RequestForQuotationVendorItem> _requestForQuotationVendorItemRepository;

        public RequestForQuotationVendorItemValidator(ILocalizationService localizationService,
            IRepository<RequestForQuotationVendor> requestForQuotationVendorRepository,
            IRepository<RequestForQuotationVendorItem> requestForQuotationVendorItemRepository)
        {
            this._requestForQuotationVendorRepository = requestForQuotationVendorRepository;
            this._requestForQuotationVendorItemRepository = requestForQuotationVendorItemRepository;

            RuleFor(x => x.QuantityQuoted).NotEmpty().When(x => x.IsAwarded == true).WithMessage(localizationService.GetResource("QuantityQuoted.Required"));
            RuleFor(x => x.UnitPriceQuoted).NotEmpty().When(x => x.IsAwarded == true).WithMessage(localizationService.GetResource("UnitPriceQuoted.Required"));
            //RuleFor(x => x).Must(NoAwardedToManyVendors).WithMessage(localizationService.GetResource("RequestForQuotation.NoAwardedToManyVendors"));
        }

        private bool NoAwardedToManyVendors(RequestForQuotationVendorItemModel model)
        {
            var rfqVendor = _requestForQuotationVendorRepository.GetById(model.RequestForQuotationVendorId);
            var vendorItems = _requestForQuotationVendorItemRepository.GetAll()
                .Where(r => r.RequestForQuotationVendor.RequestForQuotationId == rfqVendor.RequestForQuotationId
                        && r.IsAwarded == true && r.RequestForQuotationVendorId != model.RequestForQuotationVendorId)
                .ToList();
            return vendorItems.Count == 0;
        }
    }
}