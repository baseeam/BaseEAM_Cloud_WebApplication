/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using BaseEAM.Services;
using BaseEAM.Web.Framework.Validators;
using BaseEAM.Web.Models;
using System.Linq;
using FluentValidation;

namespace BaseEAM.Web.Validators
{
    public class SLADefinitionValidator : BaseEamValidator<SLADefinitionModel>
    {
        private readonly IRepository<SLADefinition> _slaDefinitionRepository;
        public SLADefinitionValidator(ILocalizationService localizationService, IRepository<SLADefinition> slaDefinitionRepository)
        {
            this._slaDefinitionRepository = slaDefinitionRepository;

            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Common.Name.Required"));
            RuleFor(x => x).Must(NoDuplication).WithMessage(localizationService.GetResource("Common.Name.Unique"));
        }

        private bool NoDuplication(SLADefinitionModel model)
        {
            var slaDefinition = _slaDefinitionRepository.GetAll().Where(c => c.Name == model.Name && c.Id != model.Id).FirstOrDefault();
            return slaDefinition == null;
        }
    }
}