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
    public class PurchaseRequestItemValidator : BaseEamValidator<PurchaseRequestItemModel>
    {
        private readonly IRepository<PurchaseRequestItem> _purchaseRequestItemRepository;
        public PurchaseRequestItemValidator(ILocalizationService localizationService, IRepository<PurchaseRequestItem> purchaseRequestItemRepository)
        {
            this._purchaseRequestItemRepository = purchaseRequestItemRepository;

            RuleFor(x => x.ItemId).NotEmpty().WithMessage(localizationService.GetResource("Item.Required"));
            //RuleFor(x => x.Sequence).NotEmpty().WithMessage(localizationService.GetResource("Common.Sequence.Required"));
            RuleFor(x => x.QuantityRequested).NotEmpty().WithMessage(localizationService.GetResource("PurchaseRequestItem.QuantityRequested.Required"));
            RuleFor(x => x).Must(NoDuplication).WithMessage(localizationService.GetResource("Item.Unique"));
        }

        private bool NoDuplication(PurchaseRequestItemModel model)
        {
            var purchaseRequestItem = _purchaseRequestItemRepository.GetAll()
                .Where(c => c.ItemId == model.ItemId && c.Id != model.Id && c.PurchaseRequestId == model.PurchaseRequestId).FirstOrDefault();
            return purchaseRequestItem == null;
        }
    }
}