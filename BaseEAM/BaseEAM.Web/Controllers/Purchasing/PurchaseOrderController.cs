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
using BaseEAM.Services.Pdf;
using BaseEAM.Web.Framework.Pdf;

namespace BaseEAM.Web.Controllers
{
    public class PurchaseOrderController : BaseController
    {
        #region Fields

        private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
        private readonly IRepository<PurchaseOrderItem> _purchaseOrderItemRepository;
        private readonly IRepository<PurchaseOrderMiscCost> _purchaseOrderMiscCostRepository;
        private readonly IRepository<Assignment> _assignmentRepository;
        private readonly IRepository<AssignmentHistory> _assignmentHistoryRepository;
        private readonly IRepository<Receipt> _receiptRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Attachment> _attachmentRepository;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IItemService _itemService;
        private readonly IMessageService _messageService;
        private readonly IAutoNumberService _autoNumberService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly HttpContextBase _httpContext;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;
        private readonly IPdfConverter _pdfConverter;

        #endregion

        #region Constructors

        public PurchaseOrderController(IRepository<PurchaseOrder> purchaseOrderRepository,
            IRepository<PurchaseOrderItem> purchaseOrderItemRepository,
            IRepository<PurchaseOrderMiscCost> purchaseOrderMiscCostRepository,
            IRepository<Assignment> assignmentRepository,
            IRepository<AssignmentHistory> assignmentHistoryRepository,
            IRepository<Receipt> receiptRepository,
            IRepository<Address> addressRepository,
            IRepository<Attachment> attachmentRepository,
            IPurchaseOrderService purchaseOrderService,
            IItemService itemService,
            IMessageService messageService,
            IAutoNumberService autoNumberService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            HttpContextBase httpContext,
            IWorkContext workContext,
            IDbContext dbContext,
            IPdfConverter pdfConverter)
        {
            this._purchaseOrderRepository = purchaseOrderRepository;
            this._purchaseOrderItemRepository = purchaseOrderItemRepository;
            this._purchaseOrderMiscCostRepository = purchaseOrderMiscCostRepository;
            this._assignmentRepository = assignmentRepository;
            this._assignmentHistoryRepository = assignmentHistoryRepository;
            this._receiptRepository = receiptRepository;
            this._addressRepository = addressRepository;
            this._attachmentRepository = attachmentRepository;
            this._localizationService = localizationService;
            this._purchaseOrderService = purchaseOrderService;
            this._itemService = itemService;
            this._messageService = messageService;
            this._autoNumberService = autoNumberService;
            this._dateTimeHelper = dateTimeHelper;
            this._permissionService = permissionService;
            this._httpContext = httpContext;
            this._workContext = workContext;
            this._dbContext = dbContext;
            this._pdfConverter = pdfConverter;
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
                DbColumn = "PurchaseOrder.Number, PurchaseOrder.Description",
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
                DbColumn = "PurchaseOrder.Priority",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int32,
                DataSource = FieldDataSource.CSV,
                CsvTextList = "Urgent,High,Medium,Low",
                CsvValueList = "0,1,2,3",
                IsRequiredField = false
            };
            model.Filters.Add(priorityFilter);

            var vendorFilter = new FieldModel
            {
                DisplayOrder = 4,
                Name = "Vendor",
                ResourceKey = "Vendor",
                DbColumn = "Company.Id",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int64,
                DataSource = FieldDataSource.MVC,
                MvcController = "Company",
                MvcAction = "VendorList",
                IsRequiredField = false
            };

            model.Filters.Add(vendorFilter);

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
                CsvTextList = "Open,WaitingForApproval,Approved,Rejected,WaitingForReceipt,Closed",
                CsvValueList = "Open,WaitingForApproval,Approved,Rejected,WaitingForReceipt,Closed",
                IsRequiredField = false
            };
            model.Filters.Add(statusFilter);

            var paymentTermFilter = new FieldModel
            {
                DisplayOrder = 6,
                Name = "PaymentTerm",
                ResourceKey = "PurchaseOrder.PaymentTerm",
                DbColumn = "ValueItem.Id",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int64,
                DataSource = FieldDataSource.MVC,
                MvcController = "Common",
                MvcAction = "ValueItems",
                AdditionalField = "category",
                AdditionalValue = "Payment Term",
                IsRequiredField = false
            };

            model.Filters.Add(paymentTermFilter);

            var expectedDeliveryDateFromFilter = new FieldModel
            {
                DisplayOrder = 7,
                Name = "ExpectedDeliveryDateFrom",
                ResourceKey = "PurchaseOrder.ExpectedDeliveryDateFrom",
                DbColumn = "PurchaseOrder.ExpectedDeliveryDate",
                Value = null,
                ControlType = FieldControlType.DateTime,
                DataType = FieldDataType.DateTime,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(expectedDeliveryDateFromFilter);

            var expectedDeliveryDateToFilter = new FieldModel
            {
                DisplayOrder = 8,
                Name = "ExpectedDeliveryDateTo",
                ResourceKey = "PurchaseOrder.ExpectedDeliveryDateTo",
                DbColumn = "PurchaseOrder.ExpectedDeliveryDate",
                Value = null,
                ControlType = FieldControlType.DateTime,
                DataType = FieldDataType.DateTime,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(expectedDeliveryDateToFilter);

            var dateOrderedFromFilter = new FieldModel
            {
                DisplayOrder = 9,
                Name = "DateOrderedFrom",
                ResourceKey = "PurchaseOrder.DateOrderedFrom",
                DbColumn = "PurchaseOrder.DateOrdered",
                Value = null,
                ControlType = FieldControlType.DateTime,
                DataType = FieldDataType.DateTime,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(dateOrderedFromFilter);

            var dateOrderedToFilter = new FieldModel
            {
                DisplayOrder = 10,
                Name = "DateOrderedTo",
                ResourceKey = "PurchaseOrder.DateOrderedTo",
                DbColumn = "PurchaseOrder.DateOrdered",
                Value = null,
                ControlType = FieldControlType.DateTime,
                DataType = FieldDataType.DateTime,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(dateOrderedToFilter);

            var dateRequiredFromFilter = new FieldModel
            {
                DisplayOrder = 9,
                Name = "DateRequiredFrom",
                ResourceKey = "PurchaseOrder.DateRequiredFrom",
                DbColumn = "PurchaseOrder.DateRequired",
                Value = null,
                ControlType = FieldControlType.DateTime,
                DataType = FieldDataType.DateTime,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(dateRequiredFromFilter);

            var dateRequiredToFilter = new FieldModel
            {
                DisplayOrder = 10,
                Name = "DateRequiredTo",
                ResourceKey = "PurchaseOrder.DateRequiredTo",
                DbColumn = "PurchaseOrder.DateRequired",
                Value = null,
                ControlType = FieldControlType.DateTime,
                DataType = FieldDataType.DateTime,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(dateRequiredToFilter);

            return model;
        }

        private SearchModel BuildCreatePurchaseOrderItemsSearchModel()
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

        #region PurchaseOrders

        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Read")]
        public ActionResult List()
        {
            var model = _httpContext.Session[SessionKey.PurchaseOrderSearchModel] as SearchModel;
            //If not exist, build search model
            if (model == null)
            {
                model = BuildSearchModel();
                //session save
                _httpContext.Session[SessionKey.PurchaseOrderSearchModel] = model;
            }
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Read")]
        public ActionResult List(DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = _httpContext.Session[SessionKey.PurchaseOrderSearchModel] as SearchModel;
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
                _httpContext.Session[SessionKey.PurchaseOrderSearchModel] = model;

                PagedResult<PurchaseOrder> data = _purchaseOrderService.GetPurchaseOrders(model.ToExpression(this._workContext.CurrentUser.Id), model.ToParameters(), command.Page - 1, command.PageSize, sort);
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
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create")]
        public ActionResult Create()
        {
            var purchaseOrder = new PurchaseOrder
            {
                IsNew = true,
                Priority = (int?)AssignmentPriority.Medium,
                CreatedUserId = this._workContext.CurrentUser.Id
            };
            _purchaseOrderRepository.InsertAndCommit(purchaseOrder);

            //start workflow
            var workflowInstanceId = WorkflowServiceClient.StartWorkflow(purchaseOrder.Id, EntityType.PurchaseOrder, 0, this._workContext.CurrentUser.Id);
            return Json(new { Id = purchaseOrder.Id });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create")]
        public ActionResult Cancel(long? parentId, long id)
        {
            var purchaseOrder = _purchaseOrderRepository.GetById(id);
            var assignment = purchaseOrder.Assignment;
            var assignmentHistories = _assignmentHistoryRepository.GetAll()
                .Where(a => a.EntityId == purchaseOrder.Id && a.EntityType == EntityType.PurchaseOrder)
                .ToList();

            _purchaseOrderRepository.Delete(purchaseOrder);
            _assignmentRepository.Delete(assignment);
            foreach (var history in assignmentHistories)
                _assignmentHistoryRepository.Delete(history);

            this._dbContext.SaveChanges();

            //cancel wf
            WorkflowServiceClient.CancelWorkflow(
                purchaseOrder.Id, EntityType.PurchaseOrder, assignment.WorkflowDefinitionId, assignment.WorkflowInstanceId,
                assignment.WorkflowVersion.Value, this._workContext.CurrentUser.Id);
            return new NullJsonResult();
        }

        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Read,Purchasing.PurchaseOrder.Update")]
        public ActionResult Edit(long id)
        {
            var purchaseOrder = _purchaseOrderRepository.GetById(id);
            var model = purchaseOrder.ToModel();
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult Edit(PurchaseOrderModel model)
        {
            var purchaseOrder = _purchaseOrderRepository.GetById(model.Id);
            var assignment = purchaseOrder.Assignment;
            if (ModelState.IsValid)
            {
                purchaseOrder = model.ToEntity(purchaseOrder);

                if (purchaseOrder.IsNew == true)
                {
                    string number = _autoNumberService.GenerateNextAutoNumber(_dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc), purchaseOrder);
                    purchaseOrder.Number = number;
                }
                //always set IsNew to false when saving
                purchaseOrder.IsNew = false;
                //copy to Assignment
                if (purchaseOrder.Assignment != null)
                {
                    purchaseOrder.Assignment.Number = purchaseOrder.Number;
                    purchaseOrder.Assignment.Description = purchaseOrder.Description;
                    purchaseOrder.Assignment.Priority = purchaseOrder.Priority;
                }
                
                var shipToAddressModel = new AddressModel
                {
                    Name = model.ShipToAddressName,
                    Country = model.ShipToAddressCountry,
                    StateProvince = model.ShipToAddressStateProvince,
                    City = model.ShipToAddressCity,
                    Address1 = model.ShipToAddressAddress1,
                    Address2 = model.ShipToAddressAddress2,
                    ZipPostalCode = model.ShipToAddressZipPostalCode,
                    PhoneNumber = model.ShipToAddressPhoneNumber,
                    FaxNumber = model.ShipToAddressFaxNumber,
                    Email = model.ShipToAddressEmail
                };

                var billToAddressModel = new AddressModel
                {
                    Name = model.BillToAddressName,
                    Country = model.BillToAddressCountry,
                    StateProvince = model.BillToAddressStateProvince,
                    City = model.BillToAddressCity,
                    Address1 = model.BillToAddressAddress1,
                    Address2 = model.BillToAddressAddress2,
                    ZipPostalCode = model.BillToAddressZipPostalCode,
                    PhoneNumber = model.BillToAddressPhoneNumber,
                    FaxNumber = model.BillToAddressFaxNumber,
                    Email = model.BillToAddressEmail
                };
                // Ship To / Bill To
                if (!model.IsNew && purchaseOrder.ShipToAddressId == null)
                {
                    // Update ShipToAddress
                    var shipToAddress = _addressRepository.GetAll().Where(a => a.Name == shipToAddressModel.Name).FirstOrDefault();
                    if (shipToAddress != null)
                    {
                        shipToAddressModel.Id = shipToAddress.Id;
                        shipToAddress = shipToAddressModel.ToEntity(shipToAddress);
                        _addressRepository.UpdateAndCommit(shipToAddress);
                    } else
                    {
                        shipToAddress = shipToAddressModel.ToEntity();
                        _addressRepository.InsertAndCommit(shipToAddress);
                    }
                    purchaseOrder.ShipToAddressId = shipToAddress.Id;
                }
                else if(purchaseOrder.ShipToAddressId > 0)
                {
                    shipToAddressModel.Id = purchaseOrder.ShipToAddressId.Value;
                    var shipToAddress = _addressRepository.GetAll().Where(a => a.Id == purchaseOrder.ShipToAddressId).FirstOrDefault();
                    shipToAddress = shipToAddressModel.ToEntity(shipToAddress);
                    _addressRepository.UpdateAndCommit(shipToAddress);

                }
                if (!model.IsNew && purchaseOrder.BillToAddressId == null)
                {
                    // Update ShipToAddress
                    var billToAddress = _addressRepository.GetAll().Where(a => a.Name == billToAddressModel.Name).FirstOrDefault();
                    if (billToAddress != null)
                    {
                        billToAddressModel.Id = billToAddress.Id;
                        billToAddress = billToAddressModel.ToEntity(billToAddress);
                        _addressRepository.UpdateAndCommit(billToAddress);
                    }
                    else
                    {
                        billToAddress = billToAddressModel.ToEntity();
                        _addressRepository.InsertAndCommit(billToAddress);
                    }
                    purchaseOrder.BillToAddressId = billToAddress.Id;
                }
                else if (purchaseOrder.BillToAddressId > 0)
                {
                    billToAddressModel.Id = purchaseOrder.BillToAddressId.Value;
                    var billToAddress = _addressRepository.GetAll().Where(a => a.Id == purchaseOrder.BillToAddressId).FirstOrDefault();
                    billToAddress = billToAddressModel.ToEntity(billToAddress);
                    _addressRepository.UpdateAndCommit(billToAddress);
                }
                _purchaseOrderRepository.Update(purchaseOrder);

                //commit all changes in UI
                this._dbContext.SaveChanges();

                //trigger workflow action
                if (!string.IsNullOrEmpty(model.ActionName))
                {
                    WorkflowServiceClient.TriggerWorkflowAction(purchaseOrder.Id, EntityType.PurchaseOrder, assignment.WorkflowDefinitionId, assignment.WorkflowInstanceId,
                        assignment.WorkflowVersion.Value, model.ActionName, model.Comment, this._workContext.CurrentUser.Id);
                    //Every time we query twice, because EF is caching entities so it won't get the latest value from DB
                    //We need to detach the specified entity and load it again
                    this._dbContext.Detach(purchaseOrder.Assignment);
                    assignment = _assignmentRepository.GetById(purchaseOrder.AssignmentId);
                }

                //notification
                SuccessNotification(_localizationService.GetResource("Record.Saved"));
                return Json(new
                {
                    number = purchaseOrder.Number,
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
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Delete")]
        public ActionResult Delete(long? parentId, long id)
        {
            var purchaseOrder = _purchaseOrderRepository.GetById(id);

            if (!_purchaseOrderService.IsDeactivable(purchaseOrder))
            {
                ModelState.AddModelError("PurchaseOrder", _localizationService.GetResource("Common.NotDeactivable"));
            }

            if (ModelState.IsValid)
            {
                var assignment = purchaseOrder.Assignment;
                var assignmentHistories = _assignmentHistoryRepository.GetAll()
                    .Where(a => a.EntityId == purchaseOrder.Id && a.EntityType == EntityType.PurchaseOrder)
                    .ToList();

                _purchaseOrderRepository.Deactivate(purchaseOrder);
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
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Delete")]
        public ActionResult DeleteSelected(long? parentId, ICollection<long> selectedIds)
        {
            var purchaseOrders = new List<PurchaseOrder>();
            foreach (long id in selectedIds)
            {
                var purchaseOrder = _purchaseOrderRepository.GetById(id);
                if (purchaseOrder != null)
                {
                    if (!_purchaseOrderService.IsDeactivable(purchaseOrder))
                    {
                        ModelState.AddModelError("PurchaseOrder", _localizationService.GetResource("Common.NotDeactivable"));
                        break;
                    }
                    else
                    {
                        purchaseOrders.Add(purchaseOrder);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                foreach (var purchaseOrder in purchaseOrders)
                {
                    var assignment = purchaseOrder.Assignment;
                    var assignmentHistories = _assignmentHistoryRepository.GetAll()
                        .Where(a => a.EntityId == purchaseOrder.Id && a.EntityType == EntityType.PurchaseOrder)
                        .ToList();

                    _purchaseOrderRepository.Deactivate(purchaseOrder);
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

        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult Preview(long id)
        {
            var purchaseOrder = _purchaseOrderRepository.GetById(id);
            var model = purchaseOrder.MapTo<PurchaseOrder, PurchaseOrderPrintModel>();
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult CreateReceipt(long purchaseOrderId)
        {
            var po = _purchaseOrderRepository.GetById(purchaseOrderId);

            //Validate
            foreach(var poItem in po.PurchaseOrderItems)
            {
                if(poItem.QuantityReceived == null || poItem.QuantityReceived == 0 || 
                    poItem.ReceiveToStoreId == null || poItem.ReceiveToStoreLocatorId == null)
                {
                    return Json(new { Errors = _localizationService.GetResource("PurchaseOrderItem.ReceiveStoreAndLocator.Required") });
                }
            }

            var receipt = _receiptRepository.GetAll()
                .Where(r => r.PurchaseOrderId == po.Id)
                .FirstOrDefault();
            if(receipt != null)
            {
                return Json(new { Errors = string.Format(_localizationService.GetResource("Receipt.AlreadyCreatedForThisPO"), po.Number) });
            }
            else
            {
                //Create receipts base on storeId 
                List<Receipt> receipts = new List<Receipt>();
                var storeIds = po.PurchaseOrderItems.Select(s => s.ReceiveToStore.Id).Distinct();
                foreach (var id in storeIds)
                {
                    var newReceipt = new Receipt();
                    newReceipt.PurchaseOrderId = purchaseOrderId;
                    newReceipt.ReceiptDate = DateTime.UtcNow;
                    newReceipt.SiteId = po.SiteId;
                    newReceipt.UserId = this._workContext.CurrentUser.Id;
                    newReceipt.Description = string.Format(_localizationService.GetResource("Receipt.ReceiptForPO"), po.Number);
                    newReceipt.StoreId = id;
                    receipts.Add(newReceipt);
                }

                //Add issue items into the issues from wois
                foreach (var poItem in po.PurchaseOrderItems)
                {
                    var newReceiptItem = new ReceiptItem();
                    newReceiptItem.StoreLocatorId = poItem.ReceiveToStoreLocatorId;
                    newReceiptItem.ItemId = poItem.ItemId;
                    newReceiptItem.ReceiptUnitOfMeasureId = poItem.Item.UnitOfMeasureId;
                    newReceiptItem.ReceiptQuantity = newReceiptItem.Quantity = poItem.QuantityReceived;
                    newReceiptItem.ReceiptUnitPrice = newReceiptItem.UnitPrice = poItem.UnitPrice;
                    newReceiptItem.Cost = poItem.QuantityReceived * poItem.UnitPrice;
                    foreach (var r in receipts)
                    {
                        if (r.StoreId == poItem.ReceiveToStoreId)
                        {
                            r.ReceiptItems.Add(newReceiptItem);
                        }
                    }
                }

                try
                {
                    foreach (var r in receipts)
                    {
                        string number = _autoNumberService.GenerateNextAutoNumber(_dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc), r);
                        r.Number = number;
                        _receiptRepository.Insert(r);
                    }

                    this._dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new { Errors = e.Message });
                }

                SuccessNotification(_localizationService.GetResource("Receipt.CreatedForPO"));
                return new NullJsonResult();
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult SendToVendor(long purchaseOrderId)
        {
            var po = _purchaseOrderRepository.GetById(purchaseOrderId);
            if (po.DateOrdered == null)
                po.DateOrdered = DateTime.UtcNow;
            _purchaseOrderRepository.UpdateAndCommit(po);

            var model = po.MapTo<PurchaseOrder, PurchaseOrderPrintModel>();

            var settings = new PdfConvertSettings
            {
                Size = PdfPageSize.A4,
                Margins = new PdfPageMargins { Top = 35, Bottom = 35 },
                Page = new PdfViewContent("Preview", model, this.ControllerContext)
            };

            var fileBytes = _pdfConverter.Convert(settings);
            // create an attachment for this PO
            var attachment = new Attachment
            {
                Name = po.Number + ".pdf",
                FileBytes = fileBytes,
                FileSize = fileBytes.Length,
                Extension = ".pdf",
                ContentType = "application/pdf"
            };
            attachment.EntityAttachments.Add(new EntityAttachment { EntityId = po.Id, EntityType = EntityType.PurchaseOrder });
            _attachmentRepository.InsertAndCommit(attachment);

            // send to vendor
            _messageService.SendEmail(
                po.Vendor.Addresses.ElementAt(0).Email,
                "",
                "",
                "",
                "admin@baseeam.com",
                "Purchase Order",
                "Please see order's details in attachment.",
                attachment.Id.ToString());

            po.IsSent = true;
            _purchaseOrderRepository.UpdateAndCommit(po);

            SuccessNotification(_localizationService.GetResource("PurchaseOrder.SentToVendor"));
            return new NullJsonResult();
        }

        #endregion

        #region PurchaseOrderItem

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Read,Purchasing.PurchaseOrder.Update")]
        public ActionResult PurchaseOrderItemList(long purchaseOrderId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _purchaseOrderItemRepository.GetAll().Where(c => c.PurchaseOrderId == purchaseOrderId);
            query = sort == null ? query.OrderBy(a => a.Sequence) : query.Sort(sort);
            var purchaseOrderItems = new PagedList<PurchaseOrderItem>(query, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = purchaseOrderItems.Select(x => x.ToModel()),
                Total = purchaseOrderItems.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Read,Purchasing.PurchaseOrder.Update")]
        public ActionResult PurchaseOrderItem(long id)
        {
            var purchaseOrderItem = _purchaseOrderItemRepository.GetById(id);
            var model = purchaseOrderItem.ToModel();
            var html = this.PurchaseOrderItemPanel(model);
            return Json(new { Id = purchaseOrderItem.Id, Html = html });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult CreatePurchaseOrderItem(long purchaseOrderId)
        {
            //need to get purchaseOrder here to assign to new purchaseOrderItem
            //so when mapping to Model, we will have StoreId as defined
            //in AutoMapper configuration
            var purchaseOrder = _purchaseOrderRepository.GetById(purchaseOrderId);
            var maxSequence = purchaseOrder.PurchaseOrderItems.Max(p => p.Sequence) ?? 0;
            var purchaseOrderItem = new PurchaseOrderItem
            {
                IsNew = true,
                PurchaseOrder = purchaseOrder,
                Sequence = maxSequence + 1
            };
            _purchaseOrderItemRepository.Insert(purchaseOrderItem);

            this._dbContext.SaveChanges();

            var model = new PurchaseOrderItemModel();
            model = purchaseOrderItem.ToModel();
            var html = this.PurchaseOrderItemPanel(model);

            return Json(new { Id = purchaseOrderItem.Id, Html = html });
        }

        [NonAction]
        public string PurchaseOrderItemPanel(PurchaseOrderItemModel model)
        {
            var html = this.RenderPartialViewToString("_LineItemDetails", model);
            return html;
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult SavePurchaseOrderItem(PurchaseOrderItemModel model)
        {
            if (ModelState.IsValid)
            {
                var purchaseOrderItem = _purchaseOrderItemRepository.GetById(model.Id);
                //always set IsNew to false when saving
                purchaseOrderItem.IsNew = false;
                purchaseOrderItem = model.ToEntity(purchaseOrderItem);
                _purchaseOrderItemRepository.UpdateAndCommit(purchaseOrderItem);
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.Errors().ToHtmlString() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult CancelPurchaseOrderItem(long id)
        {
            var purchaseOrderItem = _purchaseOrderItemRepository.GetById(id);
            if (purchaseOrderItem.IsNew == true)
            {
                _purchaseOrderItemRepository.DeleteAndCommit(purchaseOrderItem);
            }
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult DeletePurchaseOrderItem(long? parentId, long id)
        {
            var purchaseOrderItem = _purchaseOrderItemRepository.GetById(id);
            _purchaseOrderItemRepository.DeactivateAndCommit(purchaseOrderItem);
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult DeleteSelectedPurchaseOrderItems(long? parentId, long[] selectedIds)
        {
            foreach (long id in selectedIds)
            {
                var purchaseOrderItem = _purchaseOrderItemRepository.GetById(id);
                _purchaseOrderItemRepository.Deactivate(purchaseOrderItem);
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        #endregion

        #region Create PurchaseOrder Items

        [HttpGet]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult CreatePurchaseOrderItemsView()
        {
            var model = BuildCreatePurchaseOrderItemsSearchModel();
            return PartialView("_CreateLineItems", model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult CreatePurchaseOrderItemList(long purchaseOrderId, DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = BuildCreatePurchaseOrderItemsSearchModel();

            if (ModelState.IsValid)
            {
                model.Update(searchValues);

                PagedResult<Item> data = _itemService.GetItems(model.ToExpression(), model.ToParameters(), command.Page - 1, command.PageSize, sort);

                var gridModel = new DataSourceResult
                {
                    Data = data.Result.Select(x => new PurchaseOrderItemModel
                    {
                        PurchaseOrderId = purchaseOrderId,
                        ItemId = x.Id,
                        ItemName = x.Name,
                        ItemUnitOfMeasureId = x.UnitOfMeasureId,
                        ItemUnitOfMeasureName = x.UnitOfMeasure.Name,
                        UnitPrice = x.UnitPrice
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
        /// The list of creating purchaseOrder items is in updatedItems 
        /// </summary>
        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult CreatePurchaseOrderItems([Bind(Prefix = "updated")]List<PurchaseOrderItemModel> updatedItems,
           [Bind(Prefix = "created")]List<PurchaseOrderItemModel> createdItems,
           [Bind(Prefix = "deleted")]List<PurchaseOrderItemModel> deletedItems)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Create PurchaseOrderItems
                    if (updatedItems != null)
                    {
                        //get the current purchaseOrder
                        var purchaseOrderId = updatedItems.Count > 0 ? updatedItems[0].PurchaseOrderId : 0;
                        var purchaseOrderItems = _purchaseOrderItemRepository.GetAll().Where(r => r.PurchaseOrderId == purchaseOrderId).ToList();
                        var maxSequence = purchaseOrderItems.Max(p => p.Sequence) ?? 0;
                        for (int i = 1; i <= updatedItems.Count; i++)
                        {
                            var model = updatedItems[i - 1];
                            //we don't merge if the purchaseOrder item already existed
                            if (!purchaseOrderItems.Any(r => r.ItemId == model.ItemId))
                            {
                                //manually mapping here to set only foreign key
                                //if used AutoMapper, the reference property will also be mapped
                                //and EF will consider these properties as new and insert it
                                //so db records will be duplicated
                                //we can also ignore it in AutoMapper configuation instead of manually mapping
                                var purchaseOrderItem = new PurchaseOrderItem
                                {
                                    PurchaseOrderId = model.PurchaseOrderId,
                                    ItemId = model.ItemId,
                                    QuantityOrdered = model.QuantityOrdered,
                                    UnitPrice = model.UnitPrice,
                                    TaxRate = model.TaxRate,
                                    Sequence = maxSequence + i
                                };
                                purchaseOrderItem.Subtotal = purchaseOrderItem.SubtotalWithTax 
                                    = purchaseOrderItem.UnitPrice * purchaseOrderItem.QuantityOrdered;
                                if(purchaseOrderItem.TaxRate.HasValue)
                                {
                                    purchaseOrderItem.TaxAmount = (purchaseOrderItem.TaxRate * purchaseOrderItem.Subtotal) / 100;
                                    purchaseOrderItem.SubtotalWithTax = purchaseOrderItem.Subtotal + purchaseOrderItem.TaxAmount;
                                }
                                _purchaseOrderItemRepository.Insert(purchaseOrderItem);
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

        #region PurchaseOrderMiscCost

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Read,Purchasing.PurchaseOrder.Update")]
        public ActionResult PurchaseOrderMiscCostList(long purchaseOrderId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _purchaseOrderMiscCostRepository.GetAll().Where(c => c.PurchaseOrderId == purchaseOrderId);
            query = sort == null ? query.OrderBy(a => a.CreatedDateTime) : query.Sort(sort);
            var purchaseOrderMiscCosts = new PagedList<PurchaseOrderMiscCost>(query, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = purchaseOrderMiscCosts.Select(x => x.ToModel()),
                Total = purchaseOrderMiscCosts.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Read,Purchasing.PurchaseOrder.Update")]
        public ActionResult PurchaseOrderMiscCost(long id)
        {
            var purchaseOrderMiscCost = _purchaseOrderMiscCostRepository.GetById(id);
            var model = purchaseOrderMiscCost.ToModel();
            var html = this.PurchaseOrderMiscCostPanel(model);
            return Json(new { Id = purchaseOrderMiscCost.Id, Html = html });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult CreatePurchaseOrderMiscCost(long purchaseOrderId)
        {
            //need to get purchaseOrder here to assign to new purchaseOrderMiscCost
            //so when mapping to Model, we will have StoreId as defined
            //in AutoMapper configuration
            var purchaseOrder = _purchaseOrderRepository.GetById(purchaseOrderId);
            var purchaseOrderMiscCost = new PurchaseOrderMiscCost
            {
                IsNew = true,
                PurchaseOrder = purchaseOrder
            };
            _purchaseOrderMiscCostRepository.Insert(purchaseOrderMiscCost);

            this._dbContext.SaveChanges();

            var model = new PurchaseOrderMiscCostModel();
            model = purchaseOrderMiscCost.ToModel();
            var html = this.PurchaseOrderMiscCostPanel(model);

            return Json(new { Id = purchaseOrderMiscCost.Id, Html = html });
        }

        [NonAction]
        public string PurchaseOrderMiscCostPanel(PurchaseOrderMiscCostModel model)
        {
            var html = this.RenderPartialViewToString("_MiscCostDetails", model);
            return html;
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult SavePurchaseOrderMiscCost(PurchaseOrderMiscCostModel model)
        {
            if (ModelState.IsValid)
            {
                var purchaseOrderMiscCost = _purchaseOrderMiscCostRepository.GetById(model.Id);
                //always set IsNew to false when saving
                purchaseOrderMiscCost.IsNew = false;
                purchaseOrderMiscCost = model.ToEntity(purchaseOrderMiscCost);
                _purchaseOrderMiscCostRepository.UpdateAndCommit(purchaseOrderMiscCost);
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.Errors().ToHtmlString() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult CancelPurchaseOrderMiscCost(long id)
        {
            var purchaseOrderMiscCost = _purchaseOrderMiscCostRepository.GetById(id);
            if (purchaseOrderMiscCost.IsNew == true)
            {
                _purchaseOrderMiscCostRepository.DeleteAndCommit(purchaseOrderMiscCost);
            }
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult DeletePurchaseOrderMiscCost(long? parentId, long id)
        {
            var purchaseOrderMiscCost = _purchaseOrderMiscCostRepository.GetById(id);
            _purchaseOrderMiscCostRepository.DeactivateAndCommit(purchaseOrderMiscCost);
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.PurchaseOrder.Create,Purchasing.PurchaseOrder.Update")]
        public ActionResult DeleteSelectedPurchaseOrderMiscCosts(long? parentId, long[] selectedIds)
        {
            foreach (long id in selectedIds)
            {
                var purchaseOrderMiscCost = _purchaseOrderMiscCostRepository.GetById(id);
                _purchaseOrderMiscCostRepository.Deactivate(purchaseOrderMiscCost);
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        #endregion
    }
}