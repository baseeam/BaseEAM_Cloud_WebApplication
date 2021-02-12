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
    public class SLADefinitionController : BaseController
    {
        #region Fields

        private readonly IRepository<SLADefinition> _slaDefinitionRepository;
        private readonly IRepository<SLATerm> _slaTermRepository;
        private readonly IRepository<NotificationSequence> _notificationSequenceRepository;
        private readonly IRepository<SLAInstance> _slaInstanceRepository;
        private readonly IRepository<SLAInstanceTerm> _slaInstanceTermRepository;
        private readonly ISLADefinitionService _slaDefinitionService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly HttpContextBase _httpContext;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Constructors

        public SLADefinitionController(IRepository<SLADefinition> slaDefinitionRepository,
            IRepository<SLATerm> slaTermRepository,
            IRepository<NotificationSequence> notificationSequenceRepository,
            IRepository<SLAInstance> slaInstanceRepository,
            IRepository<SLAInstanceTerm> slaInstanceTermRepository,
            ISLADefinitionService slaDefinitionService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            HttpContextBase httpContext,
            IWorkContext workContext,
            IDbContext dbContext)
        {
            this._slaDefinitionRepository = slaDefinitionRepository;
            this._slaTermRepository = slaTermRepository;
            this._notificationSequenceRepository = notificationSequenceRepository;
            this._slaInstanceRepository = slaInstanceRepository;
            this._slaInstanceTermRepository = slaInstanceTermRepository;
            this._localizationService = localizationService;
            this._slaDefinitionService = slaDefinitionService;
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
            var slaDefinitionNameFilter = new FieldModel
            {
                DisplayOrder = 1,
                Name = "Name",
                ResourceKey = "SLADefinition.Name",
                DbColumn = "Name, Description",
                Value = null,
                ControlType = FieldControlType.TextBox,
                DataType = FieldDataType.String,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(slaDefinitionNameFilter);

            return model;
        }

        #endregion

        #region SLADefinitions

        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Read")]
        public ActionResult List()
        {
            var model = _httpContext.Session[SessionKey.SLADefinitionSearchModel] as SearchModel;
            //If not exist, build search model
            if (model == null)
            {
                model = BuildSearchModel();
                //session save
                _httpContext.Session[SessionKey.SLADefinitionSearchModel] = model;
            }
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Read")]
        public ActionResult List(DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = _httpContext.Session[SessionKey.SLADefinitionSearchModel] as SearchModel;
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
                _httpContext.Session[SessionKey.SLADefinitionSearchModel] = model;

                PagedResult<SLADefinition> data = _slaDefinitionService.GetSLADefinitions(model.ToExpression(), model.ToParameters(), command.Page - 1, command.PageSize, sort);

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
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Create")]
        public ActionResult Create()
        {
            var slaDefinition = new SLADefinition { IsNew = true };
            _slaDefinitionRepository.InsertAndCommit(slaDefinition);
            return Json(new { Id = slaDefinition.Id });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Create")]
        public ActionResult Cancel(long? parentId, long id)
        {
            this._dbContext.DeleteById<SLADefinition>(id);
            return new NullJsonResult();
        }

        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Create,SLADefinition.SLADefinition.Read,SLADefinition.SLADefinition.Update")]
        public ActionResult Edit(long id)
        {
            var slaDefinition = _slaDefinitionRepository.GetById(id);
            var model = slaDefinition.ToModel();
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Create,SLADefinition.SLADefinition.Update")]
        public ActionResult Edit(SLADefinitionModel model)
        {
            var slaDefinition = _slaDefinitionRepository.GetById(model.Id);
            if (ModelState.IsValid)
            {
                slaDefinition = model.ToEntity(slaDefinition);

                //always set IsNew to false when saving
                slaDefinition.IsNew = false;
                //update attributes
                _slaDefinitionRepository.Update(slaDefinition);

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
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Delete")]
        public ActionResult Delete(long? parentId, long id)
        {
            var slaDefinition = _slaDefinitionRepository.GetById(id);

            if (!_slaDefinitionService.IsDeactivable(slaDefinition))
            {
                ModelState.AddModelError("SLADefinition", _localizationService.GetResource("Common.NotDeactivable"));
            }

            if (ModelState.IsValid)
            {
                _slaDefinitionRepository.DeactivateAndCommit(slaDefinition);
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
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Delete")]
        public ActionResult DeleteSelected(long? parentId, ICollection<long> selectedIds)
        {
            var slaDefinitions = new List<SLADefinition>();
            foreach (long id in selectedIds)
            {
                var slaDefinition = _slaDefinitionRepository.GetById(id);
                if (slaDefinition != null)
                {
                    if (!_slaDefinitionService.IsDeactivable(slaDefinition))
                    {
                        ModelState.AddModelError("SLADefinition", _localizationService.GetResource("Common.NotDeactivable"));
                        break;
                    }
                    else
                    {
                        slaDefinitions.Add(slaDefinition);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                foreach (var slaDefinition in slaDefinitions)
                    _slaDefinitionRepository.Deactivate(slaDefinition);
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
        public ActionResult SLADefinitionInfo(long? slaDefinitionId)
        {
            if (slaDefinitionId == null || slaDefinitionId == 0)
                return new NullJsonResult();

            var slaDefinitionInfo = _slaDefinitionRepository.GetById(slaDefinitionId).ToModel();
            return Json(new { slaDefinitionInfo = slaDefinitionInfo });
        }

        #endregion

        #region SLATerms

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Read")]
        public ActionResult SLATermList(long slaDefinitionId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _slaTermRepository.GetAll().Where(c => c.SLADefinitionId == slaDefinitionId);
            query = sort == null ? query.OrderBy(a => a.CreatedDateTime) : query.Sort(sort);
            var slaTerms = new PagedList<SLATerm>(query, command.Page - 1, command.PageSize);
            var result = slaTerms.Select(x => x.ToModel()).ToList();

            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = slaTerms.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Create,Administration.SLADefinition.Read,Administration.SLADefinition.Update")]
        public ActionResult SLATerm(long id)
        {
            var slaTerm = _slaTermRepository.GetById(id);
            var model = slaTerm.ToModel();
            var html = this.SLATermPanel(model);
            return Json(new { Id = slaTerm.Id, Html = html });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Create")]
        public ActionResult CreateSLATerm(long slaDefinitionId)
        {
            var slaTerm = new SLATerm
            {
                IsNew = true
            };
            _slaTermRepository.Insert(slaTerm);

            var slaDefinition = _slaDefinitionRepository.GetById(slaDefinitionId);
            slaDefinition.SLATerms.Add(slaTerm);

            this._dbContext.SaveChanges();

            var model = new SLATermModel();
            model = slaTerm.ToModel();
            var html = this.SLATermPanel(model);

            return Json(new { Id = slaTerm.Id, Html = html });
        }

        [NonAction]
        public string SLATermPanel(SLATermModel model)
        {
            var html = this.RenderPartialViewToString("_SLATermDetails", model);
            return html;
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Create,Administration.SLADefinition.Update")]
        public ActionResult SaveSLATerm(SLATermModel model)
        {
            if (ModelState.IsValid)
            {
                var slaTerm = _slaTermRepository.GetById(model.Id);
                //always set IsNew to false when saving
                slaTerm.IsNew = false;

                slaTerm = model.ToEntity(slaTerm);

                _slaTermRepository.UpdateAndCommit(slaTerm);
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.Errors().ToHtmlString() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Create,Administration.SLADefinition.Update")]
        public ActionResult CancelSLATerm(long id)
        {
            var slaTerm = _slaTermRepository.GetById(id);
            if (slaTerm.IsNew == true)
            {
                _slaTermRepository.DeleteAndCommit(slaTerm);
            }
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Delete")]
        public ActionResult DeleteSLATerm(long? parentId, long id)
        {
            var slaTerm = _slaTermRepository.GetById(id);
            //For parent-child, we can mark deleted to child
            _slaTermRepository.DeactivateAndCommit(slaTerm);
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Delete")]
        public ActionResult DeleteSelectedSLATerms(long? parentId, long[] selectedIds)
        {
            foreach (long id in selectedIds)
            {
                var slaTerm = _slaTermRepository.GetById(id);
                //For parent-child, we can mark deleted to child
                _slaTermRepository.Deactivate(slaTerm);
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        #endregion

        #region Notification Sequences

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Create,Administration.SLADefinition.Read,Administration.SLADefinition.Update")]
        public ActionResult NotificationSequenceList(long slaTermId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _notificationSequenceRepository.GetAll().Where(c => c.SLATermId == slaTermId);
            query = sort == null ? query.OrderBy(a => a.Sequence) : query.Sort(sort);
            var notificationSequences = new PagedList<NotificationSequence>(query, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = notificationSequences.Select(x => x.ToModel()),
                Total = notificationSequences.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Administration.SLADefinition.Create,Administration.SLADefinition.Update")]
        public ActionResult SaveChanges([Bind(Prefix = "updated")]List<NotificationSequenceModel> updatedItems,
           [Bind(Prefix = "created")]List<NotificationSequenceModel> createdItems,
           [Bind(Prefix = "deleted")]List<NotificationSequenceModel> deletedItems)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (createdItems != null)
                    {
                        foreach (var model in createdItems)
                        {
                            var notificationSequence = model.ToEntity();
                            _notificationSequenceRepository.Insert(notificationSequence);
                        }
                    }

                    if (updatedItems != null)
                    {
                        foreach (var model in updatedItems)
                        {
                            var notificationSequence = _notificationSequenceRepository.GetById(model.Id);
                            notificationSequence = model.ToEntity(notificationSequence);
                        }
                    }

                    if (deletedItems != null)
                    {
                        foreach (var model in deletedItems)
                        {
                            var notificationSequence = _notificationSequenceRepository.GetById(model.Id);
                            _notificationSequenceRepository.Deactivate(notificationSequence);
                        }
                    }

                    _dbContext.SaveChanges();
                    SuccessNotification(_localizationService.GetResource("Record.Saved"));
                    return new NullJsonResult();
                }
                catch (System.Exception e)
                {
                    return Json(new { Errors = e.Message });
                }
            }
            else
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }
        }

        #endregion

        #region SLAInstances

        [HttpPost]
        public ActionResult SLAInstanceMeterList(long? slaInstanceId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _slaInstanceTermRepository.GetAll().Where(s => s.SLAInstanceId == slaInstanceId);
            query = sort == null ? query.OrderBy(a => a.SLAInstance.CreatedDateTime) : query.Sort(sort);
            var slaInstances = new PagedList<SLAInstanceTerm>(query, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = slaInstances.Select(x => x.ToModel()),
                Total = slaInstances.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult CreateSLAInstanceMeters(long slaDefinitionId, long entityId, string entityType)
        {
            var slaDefinition = _slaDefinitionRepository.GetById(slaDefinitionId);
            var slaInstance = _slaInstanceRepository.GetAll()
                .Where(s => s.EntityId == entityId && s.EntityType == entityType)
                .FirstOrDefault();
            if(slaInstance == null)
            {
                slaInstance = new SLAInstance
                {
                    EntityId = entityId,
                    EntityType = entityType,
                    SLADefinitionId = slaDefinitionId
                };
                _slaInstanceRepository.Insert(slaInstance);                
            }
            else
            {
                slaInstance.EntityId = entityId;
                slaInstance.EntityType = entityType;
                slaInstance.SLADefinitionId = slaDefinitionId;
                _slaInstanceRepository.Update(slaInstance);
            }

            foreach (var slaTerm in slaDefinition.SLATerms)
            {
                slaInstance.SLAInstanceTerms.Add(new SLAInstanceTerm
                {
                    SLATermId = slaTerm.Id
                });
            }

            this._dbContext.SaveChanges();

            return Json(new { slaInstanceId = slaInstance.Id });
        }

        #endregion
    }
}