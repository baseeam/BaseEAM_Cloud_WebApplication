/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core;
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using BaseEAM.Core.Kendoui;
using BaseEAM.Data;
using BaseEAM.Services;
using BaseEAM.Web.Extensions;
using BaseEAM.Web.Framework.Controllers;
using BaseEAM.Web.Framework.Mvc;
using BaseEAM.Web.Framework.CustomField;
using BaseEAM.Web.Framework.Session;
using BaseEAM.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Filters;
using System.IO;

namespace BaseEAM.Web.Controllers
{
    public class PropertyController : BaseController
    {
        #region Fields

        private readonly IRepository<Property> _propertyRepository;
        private readonly IPropertyService _propertyService;
        private readonly IEntityAttributeService _entityAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly HttpContextBase _httpContext;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Constructors

        public PropertyController(IRepository<Property> propertyRepository,
            IPropertyService propertyService,
            IEntityAttributeService entityAttributeService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            HttpContextBase httpContext,
            IWorkContext workContext,
            IDbContext dbContext)
        {
            this._propertyRepository = propertyRepository;
            this._localizationService = localizationService;
            this._propertyService = propertyService;
            this._entityAttributeService = entityAttributeService;
            this._permissionService = permissionService;
            this._httpContext = httpContext;
            this._workContext = workContext;
            this._dbContext = dbContext;
        }

        #endregion

        #region Utilities

        private SearchModel BuildSearchModel()
        {
            var model = new SearchModel();
            var propertyNameFilter = new FieldModel
            {
                DisplayOrder = 1,
                Name = "Name",
                ResourceKey = "Common.Name",
                DbColumn = "Property.Name",
                Value = null,
                ControlType = FieldControlType.TextBox,
                DataType = FieldDataType.String,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(propertyNameFilter);

            var siteFilter = new FieldModel
            {
                DisplayOrder = 2,
                Name = "Site",
                ResourceKey = "Site",
                DbColumn = "Site.Id",
                Value = this._workContext.CurrentUser.DefaultSiteId,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int64,
                DataSource = FieldDataSource.MVC,
                MvcController = "Site",
                MvcAction = "SiteList",
                IsRequiredField = false
            };
            model.Filters.Add(siteFilter);

            var locationFilter = new FieldModel
            {
                DisplayOrder = 6,
                Name = "Location",
                ResourceKey = "Location",
                DbColumn = "Location.Id",
                Value = null,
                ControlType = FieldControlType.Lookup,
                IsRequiredField = false,
                LookupType = "Location",
                LookupValueField = "LocationId",
                LookupTextField = "LocationName",
                ParentFieldName = "Site"
            };

            model.Filters.Add(locationFilter);

            return model;
        }

        #endregion

        #region Properties

        [BaseEamAuthorize(PermissionNames = "Property.Property.Read")]
        public ActionResult List()
        {
            var model = _httpContext.Session[SessionKey.PropertySearchModel] as SearchModel;
            //If not exist, build search model
            if (model == null)
            {
                model = BuildSearchModel();
                //session save
                _httpContext.Session[SessionKey.PropertySearchModel] = model;
            }
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Property.Read")]
        public ActionResult List(DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = _httpContext.Session[SessionKey.PropertySearchModel] as SearchModel;
            if (model == null)
                model = BuildSearchModel();
            else
                model.ClearValues();
            //validate
            var errorFilters = model.Validate(searchValues);
            foreach (var filter in errorFilters)
            {
                ModelState.AddModelError(filter.Name, _localizationService.GetResource(filter.ResourceKey + ".Required"));
            }
            if (ModelState.IsValid)
            {
                //session update
                model.Update(searchValues);
                _httpContext.Session[SessionKey.PropertySearchModel] = model;

                PagedResult<Property> data = _propertyService.GetProperties(model.ToExpression(this._workContext.CurrentUser.Id), model.ToParameters(), command.Page - 1, command.PageSize, sort);

                var gridModel = new DataSourceResult
                {
                    Data = data.Result.Select(x => x.ToModel()),
                    Total = data.TotalCount
                };
                return new JsonResult
                {
                    Data = gridModel
                };
            }

            return Json(new { Errors = ModelState.SerializeErrors() });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Property.Create")]
        public ActionResult Create()
        {
            var property = new Property { IsNew = true };
            _propertyRepository.InsertAndCommit(property);
            return Json(new { Id = property.Id });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Property.Create")]
        public ActionResult Cancel(long? parentId, long id)
        {
            this._dbContext.DeleteById<Property>(id);
            return new NullJsonResult();
        }

        [BaseEamAuthorize(PermissionNames = "Property.Property.Create,Property.Property.Read,Property.Property.Update")]
        public ActionResult Edit(long id)
        {
            var property = _propertyRepository.GetById(id);
            var model = property.ToModel();
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Property.Create,Property.Property.Update")]
        public ActionResult Edit(PropertyModel model)
        {
            Stream req = Request.InputStream;
            req.Seek(0, SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();

            var property = _propertyRepository.GetById(model.Id);
            if (ModelState.IsValid)
            {
                property = model.ToEntity(property);
                //always set IsNew to false when saving
                property.IsNew = false;
                _propertyRepository.Update(property);

                //update attributes
                _entityAttributeService.UpdateEntityAttributes(model.Id, EntityType.Property, json);
                _propertyRepository.Update(property);

                //commit all changes
                this._dbContext.SaveChanges();

                //notification
                SuccessNotification(_localizationService.GetResource("Record.Saved"));
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Property.Delete")]
        public ActionResult Delete(long? parentId, long id)
        {
            var property = _propertyRepository.GetById(id);

            if (!_propertyService.IsDeactivable(property))
            {
                ModelState.AddModelError("Property", _localizationService.GetResource("Common.NotDeactivable"));
            }

            if (ModelState.IsValid)
            {
                //soft delete
                _propertyRepository.DeactivateAndCommit(property);
                //notification
                SuccessNotification(_localizationService.GetResource("Record.Deleted"));
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Property.Delete")]
        public ActionResult DeleteSelected(long? parentId, ICollection<long> selectedIds)
        {
            var properties = new List<Property>();
            foreach (long id in selectedIds)
            {
                var property = _propertyRepository.GetById(id);
                if (property != null)
                {
                    if (!_propertyService.IsDeactivable(property))
                    {
                        ModelState.AddModelError("Property", _localizationService.GetResource("Common.NotDeactivable"));
                        break;
                    }
                    else
                    {
                        properties.Add(property);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                foreach (var property in properties)
                    _propertyRepository.Deactivate(property);
                this._dbContext.SaveChanges();
                SuccessNotification(_localizationService.GetResource("Record.Deleted"));
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }
        }

        #endregion
    }
}