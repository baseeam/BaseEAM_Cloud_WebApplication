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
using BaseEAM.Web.Framework.Filters;

namespace BaseEAM.Web.Controllers
{
    public class AutomatedActionController : BaseController
    {
        #region Fields

        private readonly IRepository<AutomatedAction> _automatedActionRepository;
        private readonly IAutomatedActionService _automatedActionService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly HttpContextBase _httpContext;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Constructors

        public AutomatedActionController(IRepository<AutomatedAction> automatedActionRepository,
            IAutomatedActionService automatedActionService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            HttpContextBase httpContext,
            IWorkContext workContext,
            IDbContext dbContext)
        {
            this._automatedActionRepository = automatedActionRepository;
            this._localizationService = localizationService;
            this._automatedActionService = automatedActionService;
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
            var automatedActionNameFilter = new FieldModel
            {
                DisplayOrder = 1,
                Name = "Name",
                ResourceKey = "AutomatedAction.Name",
                DbColumn = "Name, Description",
                Value = null,
                ControlType = FieldControlType.TextBox,
                DataType = FieldDataType.String,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(automatedActionNameFilter);

            return model;
        }

        #endregion

        #region AutomatedActions

        [BaseEamAuthorize(PermissionNames = "Workflow.AutomatedAction.Read")]
        public ActionResult List()
        {
            var model = _httpContext.Session[SessionKey.AutomatedActionSearchModel] as SearchModel;
            //If not exist, build search model
            if (model == null)
            {
                model = BuildSearchModel();
                //session save
                _httpContext.Session[SessionKey.AutomatedActionSearchModel] = model;
            }
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Workflow.AutomatedAction.Read")]
        public ActionResult List(DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = _httpContext.Session[SessionKey.AutomatedActionSearchModel] as SearchModel;
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
                _httpContext.Session[SessionKey.AutomatedActionSearchModel] = model;

                PagedResult<AutomatedAction> data = _automatedActionService.GetAutomatedActions(model.ToExpression(), model.ToParameters(), command.Page - 1, command.PageSize, sort);
                var result = data.Result.Select(x => x.ToModel()).ToList();
                foreach (var r in result)
                {
                    r.WhenUsedText = r.WhenUsed.ToString();
                    r.TriggerTypeText = r.TriggerType.ToString();
                }
                var gridModel = new DataSourceResult
                {
                    Data = result,
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
        [BaseEamAuthorize(PermissionNames = "Workflow.AutomatedAction.Create")]
        public ActionResult Create()
        {
            var automatedAction = new AutomatedAction { IsNew = true, TriggerType = (int?)ActionTriggerType.Immediately };
            _automatedActionRepository.InsertAndCommit(automatedAction);
            return Json(new { Id = automatedAction.Id });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Workflow.AutomatedAction.Create")]
        public ActionResult Cancel(long? parentId, long id)
        {
            this._dbContext.DeleteById<AutomatedAction>(id);
            return new NullJsonResult();
        }

        [BaseEamAuthorize(PermissionNames = "Workflow.AutomatedAction.Create,AutomatedAction.AutomatedAction.Read,AutomatedAction.AutomatedAction.Update")]
        public ActionResult Edit(long id)
        {
            var automatedAction = _automatedActionRepository.GetById(id);
            var model = automatedAction.ToModel();
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Workflow.AutomatedAction.Create,AutomatedAction.AutomatedAction.Update")]
        public ActionResult Edit(AutomatedActionModel model)
        {
            var automatedAction = _automatedActionRepository.GetById(model.Id);
            if (ModelState.IsValid)
            {
                automatedAction = model.ToEntity(automatedAction);
                if (automatedAction.TriggerType == (int?)ActionTriggerType.Immediately)
                    automatedAction.HoursAfter = 0;

                //always set IsNew to false when saving
                automatedAction.IsNew = false;
                //update attributes
                _automatedActionRepository.Update(automatedAction);

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
        [BaseEamAuthorize(PermissionNames = "Workflow.AutomatedAction.Delete")]
        public ActionResult Delete(long? parentId, long id)
        {
            var automatedAction = _automatedActionRepository.GetById(id);

            if (!_automatedActionService.IsDeactivable(automatedAction))
            {
                ModelState.AddModelError("AutomatedAction", _localizationService.GetResource("Common.NotDeactivable"));
            }

            if (ModelState.IsValid)
            {
                _automatedActionRepository.DeactivateAndCommit(automatedAction);
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
        [BaseEamAuthorize(PermissionNames = "Workflow.AutomatedAction.Delete")]
        public ActionResult DeleteSelected(long? parentId, ICollection<long> selectedIds)
        {
            var automatedActions = new List<AutomatedAction>();
            foreach (long id in selectedIds)
            {
                var automatedAction = _automatedActionRepository.GetById(id);
                if (automatedAction != null)
                {
                    if (!_automatedActionService.IsDeactivable(automatedAction))
                    {
                        ModelState.AddModelError("AutomatedAction", _localizationService.GetResource("Common.NotDeactivable"));
                        break;
                    }
                    else
                    {
                        automatedActions.Add(automatedAction);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                foreach (var automatedAction in automatedActions)
                    _automatedActionRepository.Deactivate(automatedAction);
                this._dbContext.SaveChanges();
                SuccessNotification(_localizationService.GetResource("Record.Deleted"));
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }
        }

        [HttpPost]
        public ActionResult AutomatedActionInfo(long? automatedActionId)
        {
            if (automatedActionId == null || automatedActionId == 0)
                return new NullJsonResult();

            var automatedActionInfo = _automatedActionRepository.GetById(automatedActionId).ToModel();
            return Json(new { automatedActionInfo = automatedActionInfo });
        }

        #endregion
    }
}