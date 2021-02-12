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
    public class RequestForQuotationItemValidator : BaseEamValidator<RequestForQuotationItemModel>
    {
        private readonly IRepository<RequestForQuotationItem> _purchaseRequestItemRepository;
        public RequestForQuotationItemValidator(ILocalizationService localizationService, IRepository<RequestForQuotationItem> purchaseRequestItemRepository)
        {
            this._purchaseRequestItemRepository = purchaseRequestItemRepository;

            RuleFor(x => x.ItemId).NotEmpty().WithMessage(localizationService.GetResource("Item.Required"));
            //RuleFor(x => x.Sequence).NotEmpty().WithMessage(localizationService.GetResource("Common.Sequence.Required"));
            RuleFor(x => x.QuantityRequested).NotEmpty().WithMessage(localizationService.GetResource("RequestForQuotationItem.QuantityRequested.Required"));
            RuleFor(x => x).Must(NoDuplication).WithMessage(localizationService.GetResource("Item.Unique"));
        }

        private bool NoDuplication(RequestForQuotationItemModel model)
        {
            var purchaseRequestItem = _purchaseRequestItemRepository.GetAll()
                .Where(c => c.ItemId == model.ItemId && c.Id != model.Id && c.RequestForQuotationId == model.RequestForQuotationId).FirstOrDefault();
            return purchaseRequestItem == null;
        }
    }
}