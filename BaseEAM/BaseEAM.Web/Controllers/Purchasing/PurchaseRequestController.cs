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

namespace BaseEAM.Web.Controllers
{
    public class PurchaseRequestController : BaseController
    {
        #region Fields

        private readonly IRepository<PurchaseRequest> _purchaseRequestRepository;
        private readonly IRepository<PurchaseRequestItem> _purchaseRequestItemRepository;
        private readonly IRepository<RequestForQuotation> _requestForQuotationRepository;
        private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
        private readonly IRepository<Assignment> _assignmentRepository;
        private readonly IRepository<AssignmentHistory> _assignmentHistoryRepository;
        private readonly IPurchaseRequestService _purchaseRequestService;
        private readonly IRequestForQuotationService _requestForQuotationService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IItemService _itemService;
        private readonly IAutoNumberService _autoNumberService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly HttpContextBase _httpContext;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Constructors

        public PurchaseRequestController(IRepository<PurchaseRequest> purchaseRequestRepository,
            IRepository<PurchaseRequestItem> purchaseRequestItemRepository,
            IRepository<RequestForQuotation> requestForQuotationRepository,
            IRepository<PurchaseOrder> purchaseOrderRepository,
            IRepository<Assignment> assignmentRepository,
            IRepository<AssignmentHistory> assignmentHistoryRepository,
            IPurchaseRequestService purchaseRequestService,
            IRequestForQuotationService requestForQuotationService,
            IPurchaseOrderService purchaseOrderService,
            IItemService itemService,
            IAutoNumberService autoNumberService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            HttpContextBase httpContext,
            IWorkContext workContext,
            IDbContext dbContext)
        {
            this._purchaseRequestRepository = purchaseRequestRepository;
            this._purchaseRequestItemRepository = purchaseRequestItemRepository;
            this._requestForQuotationRepository = requestForQuotationRepository;
            this._purchaseOrderRepository = purchaseOrderRepository;
            this._assignmentRepository = assignmentRepository;
            this._assignmentHistoryRepository = assignmentHistoryRepository;
            this._localizationService = localizationService;
            this._purchaseRequestService = purchaseRequestService;
            this._requestForQuotationService = requestForQuotationService;
            this._purchaseOrderService = purchaseOrderService;
            this._itemService = itemService;
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
                DbColumn = "PurchaseRequest.Number, PurchaseRequest.Description",
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
                DbColumn = "PurchaseRequest.Priority",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int32,
                DataSource = FieldDataSource.CSV,
                CsvTextList = "Urgent,High,Medium,Low",
                CsvValueList = "0,1,2,3",
                IsRequiredField = false
            };
            model.Filters.Add(priorityFilter);

            var statusFilter = new FieldModel
            {
                DisplayOrder = 4,
                Name = "Status",
                ResourceKey = "Common.Status",
                DbColumn = "Assignment.Name",
                Value = null,
                ControlType = FieldControlType.MultiSelectList,
                DataType = FieldDataType.String,
                DataSource = FieldDataSource.CSV,
                CsvTextList = "Open,WaitingForApproval,Approved,Rejected",
                CsvValueList = "Open,WaitingForApproval,Approved,Rejected",
                IsRequiredField = false
            };
            model.Filters.Add(statusFilter);

            var dateRequiredFromFilter = new FieldModel
            {
                DisplayOrder = 5,
                Name = "DateRequiredFrom",
                ResourceKey = "PurchaseRequest.DateRequiredFrom",
                DbColumn = "PurchaseRequest.DateRequired",
                Value = null,
                ControlType = FieldControlType.DateTime,
                DataType = FieldDataType.DateTime,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(dateRequiredFromFilter);

            var dateRequiredToFilter = new FieldModel
            {
                DisplayOrder = 6,
                Name = "DateRequiredTo",
                ResourceKey = "PurchaseRequest.DateRequiredTo",
                DbColumn = "PurchaseRequest.DateRequired",
                Value = null,
                ControlType = FieldControlType.DateTime,
                DataType = FieldDataType.DateTime,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(dateRequiredToFilter);

            return model;
        }

        private SearchModel BuildCreatePurchaseRequestItemsSearchModel()
        {
            var model = new SearchModel();
            var itemNameFilter = new FieldModel
            {
                DisplayOrder = 1,
                Name = "ItemName",
                ResourceKey = "Item",
                DbColumn = "Item.Name",
                Value = null,
                ControlType = FieldControlType.TextBox,
                DataType = FieldDataType.String,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(itemNameFilter);

            var itemGroupFilter = new FieldModel
            {
                DisplayOrder = 2,
                Name = "ItemGroup",
                ResourceKey = "ItemGroup",
                DbColumn = "ItemGroup.Id",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int64,
                DataSource = FieldDataSource.DB,
                DbTable = "ItemGroup",
                DbTextColumn = "Name",
                DbValueColumn = "Id",
                IsRequiredField = false
            };
            model.Filters.Add(itemGroupFilter);

            var itemCategoryFilter = new FieldModel
            {
                DisplayOrder = 3,
                Name = "ItemCategory",
                ResourceKey = "Item.ItemCategory",
                DbColumn = "Item.ItemCategory",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int64,
                DataSource = FieldDataSource.CSV,
                CsvTextList = "Part,Tool,Asset,Other",
                CsvValueList = "0,1,2,3",
                IsRequiredField = false
            };
            model.Filters.Add(itemCategoryFilter);

            var barcodeFilter = new FieldModel
            {
                DisplayOrder = 4,
                Name = "Barcode",
                ResourceKey = "Item.Barcode",
                DbColumn = "Item.Barcode",
                Value = null,
                ControlType = FieldControlType.TextBox,
                DataType = FieldDataType.String,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(barcodeFilter);

            var itemStatusFilter = new FieldModel
            {
                DisplayOrder = 5,
                Name = "ItemStatus",
                ResourceKey = "Item.ItemStatus",
                DbColumn = "Item.Id",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int64,
                DataSource = FieldDataSource.MVC,
                MvcController = "Common",
                MvcAction = "ValueItems",
                AdditionalField = "category",
                AdditionalValue = "Item Status",
                IsRequiredField = false
            };
            model.Filters.Add(itemStatusFilter);

            return model;
        }

        #endregion

        #region PurchaseRequests

        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Read")]
        public ActionResult List()
        {
            var model = _httpContext.Session[SessionKey.PurchaseRequestSearchModel] as SearchModel;
            //If not exist, build search model
            if (model == null)
            {
                model = BuildSearchModel();
                //session save
                _httpContext.Session[SessionKey.PurchaseRequestSearchModel] = model;
            }
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Read")]
        public ActionResult List(DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = _httpContext.Session[SessionKey.PurchaseRequestSearchModel] as SearchModel;
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
                _httpContext.Session[SessionKey.PurchaseRequestSearchModel] = model;

                PagedResult<PurchaseRequest> data = _purchaseRequestService.GetPurchaseRequests(model.ToExpression(this._workContext.CurrentUser.Id), model.ToParameters(), command.Page - 1, command.PageSize, sort);
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
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create")]
        public ActionResult Create()
        {
            var purchaseRequest = new PurchaseRequest
            {
                IsNew = true,
                Priority = (int?)AssignmentPriority.Medium,
                CreatedUserId = this._workContext.CurrentUser.Id
            };
            _purchaseRequestRepository.InsertAndCommit(purchaseRequest);

            //start workflow
            var workflowInstanceId = WorkflowServiceClient.StartWorkflow(purchaseRequest.Id, EntityType.PurchaseRequest, 0, this._workContext.CurrentUser.Id);
            return Json(new { Id = purchaseRequest.Id });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create")]
        public ActionResult Cancel(long? parentId, long id)
        {
            var purchaseRequest = _purchaseRequestRepository.GetById(id);
            var assignment = purchaseRequest.Assignment;
            var assignmentHistories = _assignmentHistoryRepository.GetAll()
                .Where(a => a.EntityId == purchaseRequest.Id && a.EntityType == EntityType.PurchaseRequest)
                .ToList();

            _purchaseRequestRepository.Delete(purchaseRequest);
            _assignmentRepository.Delete(assignment);
            foreach (var history in assignmentHistories)
                _assignmentHistoryRepository.Delete(history);

            this._dbContext.SaveChanges();

            //cancel wf
            WorkflowServiceClient.CancelWorkflow(
                purchaseRequest.Id, EntityType.PurchaseRequest, assignment.WorkflowDefinitionId, assignment.WorkflowInstanceId,
                assignment.WorkflowVersion.Value, this._workContext.CurrentUser.Id);
            return new NullJsonResult();
        }

        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Read,Purchasing.PurchaseRequest.Update")]
        public ActionResult Edit(long id)
        {
            var purchaseRequest = _purchaseRequestRepository.GetById(id);
            var model = purchaseRequest.ToModel();
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Update")]
        public ActionResult Edit(PurchaseRequestModel model)
        {
            var purchaseRequest = _purchaseRequestRepository.GetById(model.Id);
            var assignment = purchaseRequest.Assignment;
            if (ModelState.IsValid)
            {
                purchaseRequest = model.ToEntity(purchaseRequest);

                if (purchaseRequest.IsNew == true)
                {
                    string number = _autoNumberService.GenerateNextAutoNumber(_dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc), purchaseRequest);
                    purchaseRequest.Number = number;
                }
                //always set IsNew to false when saving
                purchaseRequest.IsNew = false;
                //copy to Assignment
                if (purchaseRequest.Assignment != null)
                {
                    purchaseRequest.Assignment.Number = purchaseRequest.Number;
                    purchaseRequest.Assignment.Description = purchaseRequest.Description;
                    purchaseRequest.Assignment.Priority = purchaseRequest.Priority;
                }

                _purchaseRequestRepository.Update(purchaseRequest);

                //commit all changes in UI
                this._dbContext.SaveChanges();

                //trigger workflow action
                if (!string.IsNullOrEmpty(model.ActionName))
                {
                    WorkflowServiceClient.TriggerWorkflowAction(purchaseRequest.Id, EntityType.PurchaseRequest, assignment.WorkflowDefinitionId, assignment.WorkflowInstanceId,
                        assignment.WorkflowVersion.Value, model.ActionName, model.Comment, this._workContext.CurrentUser.Id);
                    //Every time we query twice, because EF is caching entities so it won't get the latest value from DB
                    //We need to detach the specified entity and load it again
                    this._dbContext.Detach(purchaseRequest.Assignment);
                    assignment = _assignmentRepository.GetById(purchaseRequest.AssignmentId);
                }

                //notification
                SuccessNotification(_localizationService.GetResource("Record.Saved"));
                return Json(new
                {
                    number = purchaseRequest.Number,
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
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Delete")]
        public ActionResult Delete(long? parentId, long id)
        {
            var purchaseRequest = _purchaseRequestRepository.GetById(id);

            if (!_purchaseRequestService.IsDeactivable(purchaseRequest))
            {
                ModelState.AddModelError("PurchaseRequest", _localizationService.GetResource("Common.NotDeactivable"));
            }

            if (ModelState.IsValid)
            {
                var assignment = purchaseRequest.Assignment;
                var assignmentHistories = _assignmentHistoryRepository.GetAll()
                    .Where(a => a.EntityId == purchaseRequest.Id && a.EntityType == EntityType.PurchaseRequest)
                    .ToList();

                _purchaseRequestRepository.Deactivate(purchaseRequest);
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
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Delete")]
        public ActionResult DeleteSelected(long? parentId, ICollection<long> selectedIds)
        {
            var purchaseRequests = new List<PurchaseRequest>();
            foreach (long id in selectedIds)
            {
                var purchaseRequest = _purchaseRequestRepository.GetById(id);
                if (purchaseRequest != null)
                {
                    if (!_purchaseRequestService.IsDeactivable(purchaseRequest))
                    {
                        ModelState.AddModelError("PurchaseRequest", _localizationService.GetResource("Common.NotDeactivable"));
                        break;
                    }
                    else
                    {
                        purchaseRequests.Add(purchaseRequest);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                foreach (var purchaseRequest in purchaseRequests)
                {
                    var assignment = purchaseRequest.Assignment;
                    var assignmentHistories = _assignmentHistoryRepository.GetAll()
                        .Where(a => a.EntityId == purchaseRequest.Id && a.EntityType == EntityType.PurchaseRequest)
                        .ToList();

                    _purchaseRequestRepository.Deactivate(purchaseRequest);
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

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Update")]
        public ActionResult CreateRFQ(long purchaseRequestId)
        {
            var rfq = _requestForQuotationRepository.GetAll()
                .Where(r => r.PurchaseRequestId == purchaseRequestId)
                .FirstOrDefault();
            if(rfq != null)
            {
                return Json(new { Errors = _localizationService.GetResource("RFQ.AlreadyCreatedForPR") });
            }
            var pr = _purchaseRequestRepository.GetById(purchaseRequestId);
            try
            {
                _requestForQuotationService.CreateRFQ(pr);
            }
            catch (Exception e)
            {
                return Json(new { Errors = e.Message });
            }

            SuccessNotification(_localizationService.GetResource("RFQ.CreatedForPR"));
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Update")]
        public ActionResult CreatePO(long purchaseRequestId)
        {
            var po = _purchaseOrderRepository.GetAll()
                .Where(r => r.PurchaseRequestId == purchaseRequestId)
                .FirstOrDefault();
            if (po != null)
            {
                return Json(new { Errors = _localizationService.GetResource("PO.AlreadyCreatedForPR") });
            }
            var pr = _purchaseRequestRepository.GetById(purchaseRequestId);
            try
            {
                _purchaseOrderService.CreatePO(pr);
            }
            catch (Exception e)
            {
                return Json(new { Errors = e.Message });
            }

            SuccessNotification(_localizationService.GetResource("PO.CreatedForPR"));
            return new NullJsonResult();
        }

        #endregion

        #region PurchaseRequestItem

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Read,Purchasing.PurchaseRequest.Update")]
        public ActionResult PurchaseRequestItemList(long purchaseRequestId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _purchaseRequestItemRepository.GetAll().Where(c => c.PurchaseRequestId == purchaseRequestId);
            query = sort == null ? query.OrderBy(a => a.Sequence) : query.Sort(sort);
            var purchaseRequestItems = new PagedList<PurchaseRequestItem>(query, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = purchaseRequestItems.Select(x => x.ToModel()),
                Total = purchaseRequestItems.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Read,Purchasing.PurchaseRequest.Update")]
        public ActionResult PurchaseRequestItem(long id)
        {
            var purchaseRequestItem = _purchaseRequestItemRepository.GetById(id);
            var model = purchaseRequestItem.ToModel();
            var html = this.PurchaseRequestItemPanel(model);
            return Json(new { Id = purchaseRequestItem.Id, Html = html });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Update")]
        public ActionResult CreatePurchaseRequestItem(long purchaseRequestId)
        {
            //need to get purchaseRequest here to assign to new purchaseRequestItem
            //so when mapping to Model, we will have StoreId as defined
            //in AutoMapper configuration
            var purchaseRequest = _purchaseRequestRepository.GetById(purchaseRequestId);
            var maxSequence = purchaseRequest.PurchaseRequestItems.Max(p => p.Sequence) ?? 0;
            var purchaseRequestItem = new PurchaseRequestItem
            {
                IsNew = true,
                PurchaseRequest = purchaseRequest,
                Sequence = maxSequence + 1
            };
            _purchaseRequestItemRepository.Insert(purchaseRequestItem);

            this._dbContext.SaveChanges();

            var model = new PurchaseRequestItemModel();
            model = purchaseRequestItem.ToModel();
            var html = this.PurchaseRequestItemPanel(model);

            return Json(new { Id = purchaseRequestItem.Id, Html = html });
        }

        [NonAction]
        public string PurchaseRequestItemPanel(PurchaseRequestItemModel model)
        {
            var html = this.RenderPartialViewToString("_LineItemDetails", model);
            return html;
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Update")]
        public ActionResult SavePurchaseRequestItem(PurchaseRequestItemModel model)
        {
            if (ModelState.IsValid)
            {
                var purchaseRequestItem = _purchaseRequestItemRepository.GetById(model.Id);
                //always set IsNew to false when saving
                purchaseRequestItem.IsNew = false;
                purchaseRequestItem = model.ToEntity(purchaseRequestItem);
                _purchaseRequestItemRepository.UpdateAndCommit(purchaseRequestItem);
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.Errors().ToHtmlString() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Update")]
        public ActionResult CancelPurchaseRequestItem(long id)
        {
            var purchaseRequestItem = _purchaseRequestItemRepository.GetById(id);
            if (purchaseRequestItem.IsNew == true)
            {
                _purchaseRequestItemRepository.DeleteAndCommit(purchaseRequestItem);
            }
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Update")]
        public ActionResult DeletePurchaseRequestItem(long? parentId, long id)
        {
            var purchaseRequestItem = _purchaseRequestItemRepository.GetById(id);
            _purchaseRequestItemRepository.DeactivateAndCommit(purchaseRequestItem);
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Update")]
        public ActionResult DeleteSelectedPurchaseRequestItems(long? parentId, long[] selectedIds)
        {
            foreach (long id in selectedIds)
            {
                var purchaseRequestItem = _purchaseRequestItemRepository.GetById(id);
                _purchaseRequestItemRepository.Deactivate(purchaseRequestItem);
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        #endregion

        #region Create PurchaseRequest Items

        [HttpGet]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Update")]
        public ActionResult CreatePurchaseRequestItemsView()
        {
            var model = BuildCreatePurchaseRequestItemsSearchModel();
            return PartialView("_CreateLineItems", model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Update")]
        public ActionResult CreatePurchaseRequestItemList(long purchaseRequestId, DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = BuildCreatePurchaseRequestItemsSearchModel();

            if (ModelState.IsValid)
            {
                model.Update(searchValues);

                PagedResult<Item> data = _itemService.GetItems(model.ToExpression(), model.ToParameters(), command.Page - 1, command.PageSize, sort);

                var gridModel = new DataSourceResult
                {
                    Data = data.Result.Select(x => new PurchaseRequestItemModel
                    {
                        PurchaseRequestId = purchaseRequestId,
                        ItemId = x.Id,
                        ItemName = x.Name,
                        ItemUnitOfMeasureId = x.UnitOfMeasureId,
                        ItemUnitOfMeasureName = x.UnitOfMeasure.Name
                    }),
                    Total = data.TotalCount
                };
                return new JsonResult
                {
                    Data = gridModel
                };
            }
            else
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }
        }

        /// <summary>
        /// The list of creating purchaseRequest items is in updatedItems 
        /// </summary>
        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseRequest.Create,Purchasing.PurchaseRequest.Update")]
        public ActionResult CreatePurchaseRequestItems([Bind(Prefix = "updated")]List<PurchaseRequestItemModel> updatedItems,
           [Bind(Prefix = "created")]List<PurchaseRequestItemModel> createdItems,
           [Bind(Prefix = "deleted")]List<PurchaseRequestItemModel> deletedItems)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Create PurchaseRequestItems
                    if (updatedItems != null)
                    {
                        //get the current purchaseRequest
                        var purchaseRequestId = updatedItems.Count > 0 ? updatedItems[0].PurchaseRequestId : 0;
                        var purchaseRequestItems = _purchaseRequestItemRepository.GetAll().Where(r => r.PurchaseRequestId == purchaseRequestId).ToList();
                        var maxSequence = purchaseRequestItems.Max(p => p.Sequence) ?? 0;
                        for (int i = 1; i <= updatedItems.Count; i++)
                        {
                            var model = updatedItems[i - 1];
                            //we don't merge if the purchaseRequest item already existed
                            if (!purchaseRequestItems.Any(r => r.ItemId == model.ItemId))
                            {
                                //manually mapping here to set only foreign key
                                //if used AutoMapper, the reference property will also be mapped
                                //and EF will consider these properties as new and insert it
                                //so db records will be duplicated
                                //we can also ignore it in AutoMapper configuation instead of manually mapping
                                var purchaseRequestItem = new PurchaseRequestItem
                                {
                                    PurchaseRequestId = model.PurchaseRequestId,
                                    ItemId = model.ItemId,
                                    QuantityRequested = model.QuantityRequested,
                                    Sequence = maxSequence + i
                                };

                                _purchaseRequestItemRepository.Insert(purchaseRequestItem);
                            }
                        }
                    }

                    _dbContext.SaveChanges();
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

        #endregion
    }
}