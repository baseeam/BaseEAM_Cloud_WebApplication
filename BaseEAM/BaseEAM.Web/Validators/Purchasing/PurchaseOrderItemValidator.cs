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
    public class PurchaseOrderItemValidator : BaseEamValidator<PurchaseOrderItemModel>
    {
        private readonly IRepository<PurchaseOrderItem> _purchaseOrderItemRepository;
        public PurchaseOrderItemValidator(ILocalizationService localizationService, IRepository<PurchaseOrderItem> purchaseOrderItemRepository)
        {
            this._purchaseOrderItemRepository = purchaseOrderItemRepository;

            RuleFor(x => x.ItemId).NotEmpty().WithMessage(localizationService.GetResource("Item.Required"));
            //RuleFor(x => x.Sequence).NotEmpty().WithMessage(localizationService.GetResource("Common.Sequence.Required"));
            RuleFor(x => x.QuantityOrdered).NotEmpty().WithMessage(localizationService.GetResource("PurchaseOrderItem.QuantityOrdered.Required"));
            RuleFor(x => x.UnitPrice).NotEmpty().WithMessage(localizationService.GetResource("PurchaseOrderItem.UnitPrice.Required"));
            RuleFor(x => x).Must(NoDuplication).WithMessage(localizationService.GetResource("Item.Unique"));
        }

        private bool NoDuplication(PurchaseOrderItemModel model)
        {
            var purchaseOrderItem = _purchaseOrderItemRepository.GetAll()
                .Where(c => c.ItemId == model.ItemId && c.Id != model.Id && c.PurchaseOrderId == model.PurchaseOrderId).FirstOrDefault();
            return purchaseOrderItem == null;
        }
    }
}