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
using System;
using BaseEAM.Core.Extensions;

namespace BaseEAM.Web.Controllers
{
    public class TenantLeaseController : BaseController
    {
        #region Fields
        private readonly string[] daysOfMonth = new string[] {
                "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th", "13th", "14th", "15th",
                "16th", "17th", "18th", "19th", "20th", "21st", "22nd", "23rd", "24th", "25th", "26th", "27th", "28th", "29th*", "30th*", "31st*"
        };

        private readonly IRepository<TenantLease> _tenantLeaseRepository;
        private readonly IRepository<TenantLeasePaymentSchedule> _tenantLeasePaymentScheduleRepository;
        private readonly IRepository<TenantLeaseCharge> _tenantLeaseChargeRepository;
        private readonly IRepository<TenantPayment> _tenantPaymentRepository;
        private readonly IRepository<Assignment> _assignmentRepository;
        private readonly IRepository<AssignmentHistory> _assignmentHistoryRepository;
        private readonly ITenantLeaseService _tenantLeaseService;
        private readonly ITenantPaymentService _tenantPaymentService;
        private readonly IAutoNumberService _autoNumberService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly HttpContextBase _httpContext;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Constructors

        public TenantLeaseController(IRepository<TenantLease> tenantLeaseRepository,
            IRepository<TenantLeasePaymentSchedule> tenantLeasePaymentScheduleRepository,
            IRepository<TenantLeaseCharge> tenantLeaseChargeRepository,
            IRepository<TenantPayment> tenantPaymentRepository,
            IRepository<Assignment> assignmentRepository,
            IRepository<AssignmentHistory> assignmentHistoryRepository,
            ITenantLeaseService tenantLeaseService,
            ITenantPaymentService tenantPaymentService,
            IAutoNumberService autoNumberService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            HttpContextBase httpContext,
            IWorkContext workContext,
            IDbContext dbContext)
        {
            this._tenantLeaseRepository = tenantLeaseRepository;
            this._tenantLeasePaymentScheduleRepository = tenantLeasePaymentScheduleRepository;
            this._tenantLeaseChargeRepository = tenantLeaseChargeRepository;
            this._tenantPaymentRepository = tenantPaymentRepository;
            this._assignmentRepository = assignmentRepository;
            this._assignmentHistoryRepository = assignmentHistoryRepository;
            this._localizationService = localizationService;
            this._tenantLeaseService = tenantLeaseService;
            this._tenantPaymentService = tenantPaymentService;
            this._autoNumberService = autoNumberService;
            this._dateTimeHelper = dateTimeHelper;
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
            var numberFilter = new FieldModel
            {
                DisplayOrder = 1,
                Name = "Number",
                ResourceKey = "Common.Number",
                DbColumn = "TenantLease.Number, TenantLease.Description",
                Value = null,
                ControlType = FieldControlType.TextBox,
                DataType = FieldDataType.String,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(numberFilter);

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

            var priorityFilter = new FieldModel
            {
                DisplayOrder = 3,
                Name = "Priority",
                ResourceKey = "Priority",
                DbColumn = "TenantLease.Priority",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int32,
                DataSource = FieldDataSource.CSV,
                CsvTextList = "Urgent,High,Medium,Low",
                CsvValueList = "0,1,2,3",
                IsRequiredField = false
            };
            model.Filters.Add(priorityFilter);

            var propertyFilter = new FieldModel
            {
                DisplayOrder = 4,
                Name = "Property",
                ResourceKey = "Property",
                DbColumn = "Property.Id",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int64,
                DataSource = FieldDataSource.MVC,
                MvcController = "Site",
                MvcAction = "PropertyList",
                ParentFieldName = "Site",
                IsRequiredField = false
            };
            model.Filters.Add(propertyFilter);

            var statusFilter = new FieldModel
            {
                DisplayOrder = 5,
                Name = "Status",
                ResourceKey = "Common.Status",
                DbColumn = "Assignment.Name",
                Value = null,
                ControlType = FieldControlType.MultiSelectList,
                DataType = FieldDataType.String,
                DataSource = FieldDataSource.CSV,
                CsvTextList = "Open,Leasing,Expired,Closed",
                CsvValueList = "Open,Leasing,Expired,Closed",
                IsRequiredField = false
            };
            model.Filters.Add(statusFilter);

            var tenantFilter = new FieldModel
            {
                DisplayOrder = 6,
                Name = "Tenant",
                ResourceKey = "Tenant",
                DbColumn = "Tenant.Id",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int64,
                DataSource = FieldDataSource.DB,
                DbTable = "Tenant",
                DbTextColumn = "Name",
                DbValueColumn = "Id",
                IsRequiredField = false
            };
            model.Filters.Add(tenantFilter);

            return model;
        }

        [NonAction]
        protected virtual void PrepareTenantLeaseModel(TenantLeaseModel model)
        {
            //Term Numbers
            for (int i = 1; i <= 60; i++)
            {
                model.AvailableTermNumbers.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString(),
                    Selected = model.TermNumber == i
                });
            }

            //BiMonthly Starts
            for (int i = 1; i <= daysOfMonth.Length; i++)
            {
                model.AvailableBiMonthlyStarts.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = daysOfMonth[i - 1].ToString(),
                    Selected = model.BiMonthlyStart == i
                });
            }
            //BiMonthly Ends
            for (int i = 1; i <= daysOfMonth.Length; i++)
            {
                model.AvailableBiMonthlyEnds.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = daysOfMonth[i - 1].ToString(),
                    Selected = model.BiMonthlyEnd == i
                });
            }
        }


        [NonAction]
        protected virtual void PrepareTenantLeaseChargeModel(TenantLeaseChargeModel model)
        {
            //Charge Due Days
            for (int i = 1; i <= daysOfMonth.Length; i++)
            {
                model.AvailableChargeDueDays.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = daysOfMonth[i - 1].ToString(),
                    Selected = model.ChargeDueDay == i
                });
            }
        }

        /// <summary>
        /// Check whether Rental Term and Data has changed or not
        /// so we will re-generate payment schedules
        /// </summary>
        private bool HasChangedRentalTermAndDate(TenantLease tenantLease, TenantLeaseModel model)
        {
            var changed =  model.IsNew == false && (tenantLease.TermStartDate != model.TermStartDate
                           || tenantLease.TermNumber != model.TermNumber
                           || tenantLease.TermPeriod != (int?)model.TermPeriod
                           || tenantLease.TermIsMonthToMonth != model.TermIsMonthToMonth
                           || tenantLease.TermEndDate != model.TermEndDate
                           || tenantLease.TermRentAmount != model.TermRentAmount
                           || tenantLease.DueFrequency != (int?)model.DueFrequency
                           || tenantLease.FirstPaymentDate != model.FirstPaymentDate);

            return changed;
        }

        /// <summary>
        /// make sure that the tenant lease has not
        /// any payment which is collected
        /// </summary>
        /// <param name="tenantLease"></param>
        /// <returns></returns>
        private bool HasNotAnyPaymentCollectionOrDaysLateInTenantPayment(List<TenantPayment> tenantPayments)
        {
            foreach (var tenantPayment in tenantPayments)
            {
                if (tenantPayment.DaysLateCount > 0)
                {
                    return false;
                }
                var paymentCollections = tenantPayment.TenantPaymentCollections;
                if (paymentCollections != null && paymentCollections.Count() > 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// The ChargePayment field has changed or not
        /// </summary>
        /// <param name="prevTenantLease"></param>
        /// <param name="curTenantLease"></param>
        /// <returns></returns>
        private bool HasChangedChargePayment(TenantLeaseCharge tenantLeaseCharge, TenantLeaseChargeModel model)
        {
            var changed = model.IsNew == false && (tenantLeaseCharge.ChargeTypeId != model.ChargeTypeId
                           ||tenantLeaseCharge.ChargeAmount != model.ChargeAmount
                           || tenantLeaseCharge.ChargeDueType != (int?)model.ChargeDueType
                           || tenantLeaseCharge.ChargeDueDate != model.ChargeDueDate
                           || tenantLeaseCharge.ChargeDueDay != model.ChargeDueDay
                           || tenantLeaseCharge.ValidFrom != model.ValidFrom
                           || tenantLeaseCharge.ValidTo != model.ValidTo);

            return changed;
        }
        #endregion

        #region TenantLeases

        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Read")]
        public ActionResult List()
        {
            var model = _httpContext.Session[SessionKey.TenantLeaseSearchModel] as SearchModel;
            //If not exist, build search model
            if (model == null)
            {
                model = BuildSearchModel();
                //session save
                _httpContext.Session[SessionKey.TenantLeaseSearchModel] = model;
            }
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Read")]
        public ActionResult List(DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = _httpContext.Session[SessionKey.TenantLeaseSearchModel] as SearchModel;
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
                _httpContext.Session[SessionKey.TenantLeaseSearchModel] = model;

                PagedResult<TenantLease> data = _tenantLeaseService.GetTenantLeases(model.ToExpression(this._workContext.CurrentUser.Id), model.ToParameters(), command.Page - 1, command.PageSize, sort);
                var result = data.Result.Select(x => x.ToModel()).ToList();
                foreach (var item in result)
                {
                    item.PriorityText = item.Priority.ToString();
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
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Create")]
        public ActionResult Create()
        {
            var tenantLease = new TenantLease
            {
                IsNew = true,
                Priority = (int?)AssignmentPriority.Medium,
                CreatedUserId = this._workContext.CurrentUser.Id

            };
            _tenantLeaseRepository.InsertAndCommit(tenantLease);

            //start workflow
            var workflowInstanceId = WorkflowServiceClient.StartWorkflow(tenantLease.Id, EntityType.TenantLease, 0, this._workContext.CurrentUser.Id);
            return Json(new { Id = tenantLease.Id });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Create")]
        public ActionResult Cancel(long? parentId, long id)
        {
            var tenantLease = _tenantLeaseRepository.GetById(id);
            var assignment = tenantLease.Assignment;
            var assignmentHistories = _assignmentHistoryRepository.GetAll()
                .Where(a => a.EntityId == tenantLease.Id && a.EntityType == EntityType.TenantLease)
                .ToList();

            _tenantLeaseRepository.Delete(tenantLease);
            _assignmentRepository.Delete(assignment);
            foreach (var history in assignmentHistories)
                _assignmentHistoryRepository.Delete(history);

            this._dbContext.SaveChanges();

            //cancel wf
            WorkflowServiceClient.CancelWorkflow(
                tenantLease.Id, EntityType.TenantLease, assignment.WorkflowDefinitionId, assignment.WorkflowInstanceId,
                assignment.WorkflowVersion.Value, this._workContext.CurrentUser.Id);
            return new NullJsonResult();
        }

        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Create,Property.TenantLease.Read,Property.TenantLease.Update")]
        public ActionResult Edit(long id)
        {
            var tenantLease = _tenantLeaseRepository.GetById(id);
            var model = tenantLease.ToModel();
            PrepareTenantLeaseModel(model);
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Create,Property.TenantLease.Update")]
        public ActionResult Edit(TenantLeaseModel model)
        {
            var tenantLease = _tenantLeaseRepository.GetById(model.Id);
            var assignment = tenantLease.Assignment;
            //find the list of leases status are leasing
            var existingLeases = _tenantLeaseRepository.GetAll().Where(l => l.SiteId == model.SiteId && l.PropertyId == model.PropertyId && l.TenantId == model.TenantId && l.Assignment.Name == "Leasing" && l.Id != model.Id).ToList();
            if (existingLeases.Count > 0)
            {
                ModelState.AddModelError("TenantLease", _localizationService.GetResource("TenantLease.CannotCreateNewLease"));
            }

            if (ModelState.IsValid)
            {
                var hasChangedRentalTermAndDate = HasChangedRentalTermAndDate(tenantLease, model);

                tenantLease = model.ToEntity(tenantLease);

                if (tenantLease.IsNew == true)
                {
                    string number = _autoNumberService.GenerateNextAutoNumber(_dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc), tenantLease);
                    tenantLease.Number = number;
                }
                //always set IsNew to false when saving
                tenantLease.IsNew = false;
                //copy to Assignment
                if (tenantLease.Assignment != null)
                {
                    tenantLease.Assignment.Number = tenantLease.Number;
                    tenantLease.Assignment.Description = tenantLease.Description;
                    tenantLease.Assignment.Priority = tenantLease.Priority;
                }

                _tenantLeaseRepository.Update(tenantLease);

                //commit all changes in UI
                this._dbContext.SaveChanges();

                //trigger workflow action
                if (!string.IsNullOrEmpty(model.ActionName))
                {
                    WorkflowServiceClient.TriggerWorkflowAction(tenantLease.Id, EntityType.TenantLease, assignment.WorkflowDefinitionId, assignment.WorkflowInstanceId,
                        assignment.WorkflowVersion.Value, model.ActionName, model.Comment, this._workContext.CurrentUser.Id);
                    //Every time we query twice, because EF is caching entities so it won't get the latest value from DB
                    //We need to detach the specified entity and load it again
                    this._dbContext.Detach(tenantLease.Assignment);
                    assignment = _assignmentRepository.GetById(tenantLease.AssignmentId);
                }

                //Generate Payment Schedule and Payment
                this._dbContext.Detach(tenantLease);
                tenantLease = _tenantLeaseRepository.GetById(tenantLease.Id);

                if (hasChangedRentalTermAndDate)
                {
                    var tenantPayments = tenantLease.TenantPayments.Where(p => p.TenantLeasePaymentScheduleId != null).ToList();
                    var hasNotAnyPaymentCollectionOrDaysLateInTenantPayment = HasNotAnyPaymentCollectionOrDaysLateInTenantPayment(tenantPayments);
                    if (hasNotAnyPaymentCollectionOrDaysLateInTenantPayment)
                    {
                        _tenantLeaseService.GeneratePaymentSchedules(tenantLease);

                        var firstPaymentSchedulePeriod = _tenantLeasePaymentScheduleRepository.GetAll()
                            .Where(s => s.TenantLeaseId == tenantLease.Id)
                            .OrderBy(s => s.DueDate)
                            .FirstOrDefault();
                        _tenantLeaseService.CreateRentPayment(firstPaymentSchedulePeriod, tenantLease.FirstPaymentDate);
                        this._dbContext.SaveChanges();
                    }
                    else
                    {
                        return Json(new { Errors = _localizationService.GetResource("TenantLease.CannotGeneratePayment") });
                    }
                }

                //notification
                SuccessNotification(_localizationService.GetResource("Record.Saved"));
                return Json(new
                {
                    number = tenantLease.Number,
                    status = assignment.Name,
                    assignedUsers = assignment.Users.Select(u => u.Name),
                    availableActions = assignment.AvailableActions ?? ""
                });
            }
            else
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Delete")]
        public ActionResult Delete(long? parentId, long id)
        {
            var tenantLease = _tenantLeaseRepository.GetById(id);

            if (!_tenantLeaseService.IsDeactivable(tenantLease))
            {
                ModelState.AddModelError("TenantLease", _localizationService.GetResource("Common.NotDeactivable"));
            }

            if (ModelState.IsValid)
            {
                var assignment = tenantLease.Assignment;
                var assignmentHistories = _assignmentHistoryRepository.GetAll()
                    .Where(a => a.EntityId == tenantLease.Id && a.EntityType == EntityType.TenantLease)
                    .ToList();

                _tenantLeaseRepository.Deactivate(tenantLease);
                _assignmentRepository.Deactivate(assignment);
                foreach (var history in assignmentHistories)
                    _assignmentHistoryRepository.Deactivate(history);

                this._dbContext.SaveChanges();

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
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Delete")]
        public ActionResult DeleteSelected(long? parentId, ICollection<long> selectedIds)
        {
            var tenantLeases = new List<TenantLease>();
            foreach (long id in selectedIds)
            {
                var tenantLease = _tenantLeaseRepository.GetById(id);
                if (tenantLease != null)
                {
                    if (!_tenantLeaseService.IsDeactivable(tenantLease))
                    {
                        ModelState.AddModelError("TenantLease", _localizationService.GetResource("Common.NotDeactivable"));
                        break;
                    }
                    else
                    {
                        tenantLeases.Add(tenantLease);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                foreach (var tenantLease in tenantLeases)
                {
                    var assignment = tenantLease.Assignment;
                    var assignmentHistories = _assignmentHistoryRepository.GetAll()
                        .Where(a => a.EntityId == tenantLease.Id && a.EntityType == EntityType.TenantLease)
                        .ToList();

                    _tenantLeaseRepository.Deactivate(tenantLease);
                    _assignmentRepository.Deactivate(assignment);
                    foreach (var history in assignmentHistories)
                        _assignmentHistoryRepository.Deactivate(history);
                }
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

        #region TenantLeaseCharge

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Create,Property.TenantLease.Read,Property.TenantLease.Update,Property.Tenant.Portal")]
        public ActionResult TenantLeaseChargeList(long tenantLeaseId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _tenantLeaseChargeRepository.GetAll().Where(c => c.TenantLeaseId == tenantLeaseId);
            query = sort == null ? query.OrderBy(a => a.Name) : query.Sort(sort);
            var tenantLeaseCharges = new PagedList<TenantLeaseCharge>(query, command.Page - 1, command.PageSize);

            var result = tenantLeaseCharges.Select(x => x.ToModel()).ToList();
            foreach (var r in result)
            {
                r.ChargeDueTypeText = EnumExtensions.GetDisplayName(r.ChargeDueType);
            }
            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = tenantLeaseCharges.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Create,Property.TenantLease.Read,Property.TenantLease.Update")]
        public ActionResult TenantLeaseCharge(long id)
        {
            var tenantLeaseCharge = _tenantLeaseChargeRepository.GetById(id);
            var model = tenantLeaseCharge.ToModel();
            PrepareTenantLeaseChargeModel(model);
            var html = this.TenantLeaseChargePanel(model);
            return Json(new { Id = tenantLeaseCharge.Id, Html = html, ChargeDueType = model.ChargeDueType });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Create,Property.TenantLease.Update")]
        public ActionResult CreateTenantLeaseCharge(long tenantLeaseId)
        {
            //need to get tenantLease here to assign to new tenantLeaseCharge
            //so when mapping to Model, we will have StoreId as defined
            //in AutoMapper configuration
            var tenantLease = _tenantLeaseRepository.GetById(tenantLeaseId);
            var tenantLeaseCharge = new TenantLeaseCharge
            {
                IsNew = true,
                TenantLease = tenantLease
            };
            _tenantLeaseChargeRepository.Insert(tenantLeaseCharge);

            this._dbContext.SaveChanges();

            var model = new TenantLeaseChargeModel();
            model = tenantLeaseCharge.ToModel();
            PrepareTenantLeaseChargeModel(model);
            var html = this.TenantLeaseChargePanel(model);

            return Json(new { Id = tenantLeaseCharge.Id, Html = html, ChargeDueType = model.ChargeDueType });
        }

        [NonAction]
        public string TenantLeaseChargePanel(TenantLeaseChargeModel model)
        {
            var html = this.RenderPartialViewToString("_TenantLeaseChargeDetails", model);
            return html;
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Create,Property.TenantLease.Update")]
        public ActionResult SaveTenantLeaseCharge(TenantLeaseChargeModel model)
        {
            if (ModelState.IsValid)
            {
                var tenantLease = _tenantLeaseRepository.GetById(model.TenantLeaseId);
                var tenantLeaseCharge = _tenantLeaseChargeRepository.GetById(model.Id);
                var changed = HasChangedChargePayment(tenantLeaseCharge, model);
                //always set IsNew to false when saving
                tenantLeaseCharge.IsNew = false;
                tenantLeaseCharge = model.ToEntity(tenantLeaseCharge);
                _tenantLeaseChargeRepository.UpdateAndCommit(tenantLeaseCharge);

                if (changed)
                {
                    var tenantPayments = _tenantPaymentRepository.GetAll()
                        .Where(p => p.TenantLeaseChargeId.HasValue && p.TenantLeaseId == model.TenantLeaseId && p.ChargeTypeId == model.ChargeTypeId)
                        .ToList();

                    var hasNotAnyPaymentCollectionOrDaysLateInTenantPayment = HasNotAnyPaymentCollectionOrDaysLateInTenantPayment(tenantPayments);
                    if (hasNotAnyPaymentCollectionOrDaysLateInTenantPayment)
                    {
                        foreach (var tenantPayment in tenantPayments)
                        {
                            _tenantPaymentService.DeletePayment(tenantPayment);
                        }
                        var dueDate = _tenantLeaseService.GetDueDate(tenantLease, tenantLeaseCharge);
                        _tenantLeaseService.CreateChargePayment(tenantLeaseCharge, dueDate);
                        this._dbContext.SaveChanges();
                    }
                    else
                    {
                        return Json(new { Errors = _localizationService.GetResource("TenantLease.CannotGeneratePayment") });
                    }
                }
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.Errors().ToHtmlString() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Create,Property.TenantLease.Update")]
        public ActionResult CancelTenantLeaseCharge(long id)
        {
            var tenantLeaseCharge = _tenantLeaseChargeRepository.GetById(id);
            if (tenantLeaseCharge.IsNew == true)
            {
                _tenantLeaseChargeRepository.DeleteAndCommit(tenantLeaseCharge);
            }
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Create,Property.TenantLease.Update")]
        public ActionResult DeleteTenantLeaseCharge(long? parentId, long id)
        {
            var tenantLeaseCharge = _tenantLeaseChargeRepository.GetById(id);
            _tenantLeaseChargeRepository.DeactivateAndCommit(tenantLeaseCharge);
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Create,Property.TenantLease.Update")]
        public ActionResult DeleteSelectedTenantLeaseCharges(long? parentId, long[] selectedIds)
        {
            foreach (long id in selectedIds)
            {
                var tenantLeaseCharge = _tenantLeaseChargeRepository.GetById(id);
                _tenantLeaseChargeRepository.Deactivate(tenantLeaseCharge);
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        #endregion

        #region TenantLeasePaymentSchedule

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Read,Property.TenantLease.Update")]
        public ActionResult TenantLeasePaymentScheduleList(long tenantLeaseId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _tenantLeasePaymentScheduleRepository.GetAll().Where(c => c.TenantLeaseId == tenantLeaseId);
            query = sort == null ? query.OrderBy(a => a.DueDate) : query.Sort(sort);
            var tenantLeasePaymentSchedules = new PagedList<TenantLeasePaymentSchedule>(query, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = tenantLeasePaymentSchedules.Select(x => x.ToModel()),
                Total = tenantLeasePaymentSchedules.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantLease.Create,Property.TenantLease.Update,Property.TenantLease.Delete")]
        public ActionResult SaveChanges([Bind(Prefix = "updated")]List<TenantLeasePaymentScheduleModel> updatedItems,
          [Bind(Prefix = "created")]List<TenantLeasePaymentScheduleModel> createdItems,
          [Bind(Prefix = "deleted")]List<TenantLeasePaymentScheduleModel> deletedItems)
        {
            Validate(updatedItems);
            if (ModelState.IsValid)
            {
                try
                {
                    //Update TenantLeasePaymentSchedule
                    if (updatedItems != null)
                    {
                        foreach (var model in updatedItems)
                        {
                            var tenantPaymentSchedule = _tenantLeasePaymentScheduleRepository.GetById(model.Id);
                            tenantPaymentSchedule.DueDate = model.DueDate;
                            tenantPaymentSchedule.DueAmount = model.DueAmount;
                            _tenantLeasePaymentScheduleRepository.Update(tenantPaymentSchedule);

                            var existingPayments = _tenantPaymentRepository.GetAll().Where(p => p.TenantLeasePaymentScheduleId == tenantPaymentSchedule.Id).ToList();
                            if (existingPayments.Count > 0)
                            {
                                if (HasNotAnyPaymentCollectionOrDaysLateInTenantPayment(existingPayments))
                                {
                                    var existingPayment = existingPayments.FirstOrDefault();
                                    existingPayment.DueAmount = model.DueAmount;
                                    existingPayment.DueDate = model.DueDate;
                                    _tenantPaymentService.UpdatePayment(existingPayment);
                                }

                                else
                                {
                                    return Json(new { Errors = _localizationService.GetResource("TenantLease.CannotGeneratePayment") });
                                }
                            }
                        }
                        _dbContext.SaveChanges();
                    }
                    SuccessNotification(_localizationService.GetResource("Record.Saved"));
                    return new NullJsonResult();
                }
                catch (Exception e)
                {
                    return Json(new { Errors = e.Message });
                }
            }
            else
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }
        }

        private void Validate(List<TenantLeasePaymentScheduleModel> models)
        {
            foreach (var model in models)
            {
                var tenantLease = _tenantLeaseRepository.GetById(model.TenantLeaseId);
                if(model.DueDate.HasValue && (model.DueDate < tenantLease.TermStartDate || model.DueDate > tenantLease.TermEndDate))
                {
                    ModelState.AddModelError("", _localizationService.GetResource("TenantLeasePaymentSchedule.DueDate.InRangeTermStartDateAndTermEndDate"));
                    return;
                }
                //validate items should not be collected or not
                var tenantPayment = _tenantPaymentRepository.GetAll().Where(p => p.TenantLeasePaymentScheduleId == model.Id && p.TenantPaymentCollections.Count > 0).FirstOrDefault();
                if(tenantPayment != null){
                    ModelState.AddModelError("", _localizationService.GetResource("TenantLeasePaymentSchedule.TenantPayment.HasBeenCollected"));
                    return;
                }
             }
        }
        #endregion

    }
}