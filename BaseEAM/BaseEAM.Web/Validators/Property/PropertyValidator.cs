using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using BaseEAM.Services;
using BaseEAM.Web.Framework.Validators;
using BaseEAM.Web.Models;
using FluentValidation;
using System.Linq;

namespace BaseEAM.Web.Validators
{
    public class PropertyValidator : BaseEamValidator<PropertyModel>
    {
        private readonly IRepository<Property> _propertyRepository;
        public PropertyValidator(ILocalizationService localizationService, IRepository<Property> propertyRepository)
        {
            this._propertyRepository = propertyRepository;

            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Property.Name.Required"));
            RuleFor(x => x.SiteId).NotEmpty().WithMessage(localizationService.GetResource("Site.Required"));
            RuleFor(x => x.LocationId).NotEmpty().WithMessage(localizationService.GetResource("Location.Required"));
            RuleFor(x => x).Must(NoDuplication).WithMessage(localizationService.GetResource("Property.Name.Unique"));
        }

        private bool NoDuplication(PropertyModel model)
        {
            var property = _propertyRepository.GetAll().Where(c => c.Name == model.Name && c.Id != model.Id).FirstOrDefault();
            return property == null;
        }
    }
}