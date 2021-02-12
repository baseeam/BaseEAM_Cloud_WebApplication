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
    public class RequestForQuotationController : BaseController
    {
        #region Fields

        private readonly IRepository<RequestForQuotation> _requestForQuotationRepository;
        private readonly IRepository<RequestForQuotationItem> _requestForQuotationItemRepository;
        private readonly IRepository<RequestForQuotationVendor> _requestForQuotationVendorRepository;
        private readonly IRepository<RequestForQuotationVendorItem> _requestForQuotationVendorItemRepository;
        private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Assignment> _assignmentRepository;
        private readonly IRepository<AssignmentHistory> _assignmentHistoryRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Attachment> _attachmentRepository;
        private readonly IRequestForQuotationService _requestForQuotationService;
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

        public RequestForQuotationController(IRepository<RequestForQuotation> requestForQuotationRepository,
            IRepository<RequestForQuotationItem> requestForQuotationItemRepository,
            IRepository<RequestForQuotationVendor> requestForQuotationVendorRepository,
            IRepository<RequestForQuotationVendorItem> requestForQuotationVendorItemRepository,
            IRepository<PurchaseOrder> purchaseOrderRepository,
            IRepository<Company> companyRepository,
            IRepository<Assignment> assignmentRepository,
            IRepository<AssignmentHistory> assignmentHistoryRepository,
            IRepository<Address> addressRepository,
            IRepository<Attachment> attachmentRepository,
            IRequestForQuotationService requestForQuotationService,
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
            this._requestForQuotationRepository = requestForQuotationRepository;
            this._requestForQuotationItemRepository = requestForQuotationItemRepository;
            this._requestForQuotationVendorRepository = requestForQuotationVendorRepository;
            this._requestForQuotationVendorItemRepository = requestForQuotationVendorItemRepository;
            this._purchaseOrderRepository = purchaseOrderRepository;
            this._companyRepository = companyRepository;
            this._assignmentRepository = assignmentRepository;
            this._assignmentHistoryRepository = assignmentHistoryRepository;
            this._addressRepository = addressRepository;
            this._attachmentRepository = attachmentRepository;
            this._localizationService = localizationService;
            this._requestForQuotationService = requestForQuotationService;
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
                DbColumn = "RequestForQuotation.Number, RequestForQuotation.Description",
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
                DbColumn = "RequestForQuotation.Priority",
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
                ResourceKey = "RequestForQuotation.DateRequiredFrom",
                DbColumn = "RequestForQuotation.DateRequired",
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
                ResourceKey = "RequestForQuotation.DateRequiredTo",
                DbColumn = "RequestForQuotation.DateRequired",
                Value = null,
                ControlType = FieldControlType.DateTime,
                DataType = FieldDataType.DateTime,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(dateRequiredToFilter);

            var expectedQuoteDateFromFilter = new FieldModel
            {
                DisplayOrder = 7,
                Name = "ExpectedQuoteDateFrom",
                ResourceKey = "RequestForQuotation.ExpectedQuoteDateFrom",
                DbColumn = "RequestForQuotation.ExpectedQuoteDate",
                Value = null,
                ControlType = FieldControlType.DateTime,
                DataType = FieldDataType.DateTime,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(expectedQuoteDateFromFilter);

            var expectedQuoteDateToFilter = new FieldModel
            {
                DisplayOrder = 8,
                Name = "ExpectedQuoteDateTo",
                ResourceKey = "RequestForQuotation.ExpectedQuoteDateTo",
                DbColumn = "RequestForQuotation.ExpectedQuoteDate",
                Value = null,
                ControlType = FieldControlType.DateTime,
                DataType = FieldDataType.DateTime,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(expectedQuoteDateToFilter);

            return model;
        }

        private SearchModel BuildCreateRequestForQuotationItemsSearchModel()
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

        #region RequestForQuotations

        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Read")]
        public ActionResult List()
        {
            var model = _httpContext.Session[SessionKey.RequestForQuotationSearchModel] as SearchModel;
            //If not exist, build search model
            if (model == null)
            {
                model = BuildSearchModel();
                //session save
                _httpContext.Session[SessionKey.RequestForQuotationSearchModel] = model;
            }
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Read")]
        public ActionResult List(DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = _httpContext.Session[SessionKey.RequestForQuotationSearchModel] as SearchModel;
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
                _httpContext.Session[SessionKey.RequestForQuotationSearchModel] = model;

                PagedResult<RequestForQuotation> data = _requestForQuotationService.GetRequestForQuotations(model.ToExpression(this._workContext.CurrentUser.Id), model.ToParameters(), command.Page - 1, command.PageSize, sort);
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
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create")]
        public ActionResult Create()
        {
            var requestForQuotation = new RequestForQuotation
            {
                IsNew = true,
                Priority = (int?)AssignmentPriority.Medium,
                CreatedUserId = this._workContext.CurrentUser.Id
            };
            _requestForQuotationRepository.InsertAndCommit(requestForQuotation);

            //start workflow
            var workflowInstanceId = WorkflowServiceClient.StartWorkflow(requestForQuotation.Id, EntityType.RequestForQuotation, 0, this._workContext.CurrentUser.Id);
            return Json(new { Id = requestForQuotation.Id });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create")]
        public ActionResult Cancel(long? parentId, long id)
        {
            var requestForQuotation = _requestForQuotationRepository.GetById(id);
            var assignment = requestForQuotation.Assignment;
            var assignmentHistories = _assignmentHistoryRepository.GetAll()
                .Where(a => a.EntityId == requestForQuotation.Id && a.EntityType == EntityType.RequestForQuotation)
                .ToList();

            _requestForQuotationRepository.Delete(requestForQuotation);
            _assignmentRepository.Delete(assignment);
            foreach (var history in assignmentHistories)
                _assignmentHistoryRepository.Delete(history);

            this._dbContext.SaveChanges();

            //cancel wf
            WorkflowServiceClient.CancelWorkflow(
                requestForQuotation.Id, EntityType.RequestForQuotation, assignment.WorkflowDefinitionId, assignment.WorkflowInstanceId,
                assignment.WorkflowVersion.Value, this._workContext.CurrentUser.Id);
            return new NullJsonResult();
        }

        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Read,Purchasing.RequestForQuotation.Update")]
        public ActionResult Edit(long id)
        {
            var requestForQuotation = _requestForQuotationRepository.GetById(id);
            var model = requestForQuotation.ToModel();
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult Edit(RequestForQuotationModel model)
        {
            var requestForQuotation = _requestForQuotationRepository.GetById(model.Id);
            var assignment = requestForQuotation.Assignment;
            if (ModelState.IsValid)
            {
                requestForQuotation = model.ToEntity(requestForQuotation);

                if (requestForQuotation.IsNew == true)
                {
                    string number = _autoNumberService.GenerateNextAutoNumber(_dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc), requestForQuotation);
                    requestForQuotation.Number = number;
                }
                //always set IsNew to false when saving
                requestForQuotation.IsNew = false;
                //copy to Assignment
                if (requestForQuotation.Assignment != null)
                {
                    requestForQuotation.Assignment.Number = requestForQuotation.Number;
                    requestForQuotation.Assignment.Description = requestForQuotation.Description;
                    requestForQuotation.Assignment.Priority = requestForQuotation.Priority;
                }
                // Ship To
                if (requestForQuotation.ShipToAddressId == null)
                {
                    requestForQuotation.ShipToAddress = new Address
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
                }
                _requestForQuotationRepository.Update(requestForQuotation);

                //commit all changes in UI
                this._dbContext.SaveChanges();

                //trigger workflow action
                if (!string.IsNullOrEmpty(model.ActionName))
                {
                    WorkflowServiceClient.TriggerWorkflowAction(requestForQuotation.Id, EntityType.RequestForQuotation, assignment.WorkflowDefinitionId, assignment.WorkflowInstanceId,
                        assignment.WorkflowVersion.Value, model.ActionName, model.Comment, this._workContext.CurrentUser.Id);
                    //Every time we query twice, because EF is caching entities so it won't get the latest value from DB
                    //We need to detach the specified entity and load it again
                    this._dbContext.Detach(requestForQuotation.Assignment);
                    assignment = _assignmentRepository.GetById(requestForQuotation.AssignmentId);
                }

                //notification
                SuccessNotification(_localizationService.GetResource("Record.Saved"));
                return Json(new
                {
                    number = requestForQuotation.Number,
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
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Delete")]
        public ActionResult Delete(long? parentId, long id)
        {
            var requestForQuotation = _requestForQuotationRepository.GetById(id);

            if (!_requestForQuotationService.IsDeactivable(requestForQuotation))
            {
                ModelState.AddModelError("RequestForQuotation", _localizationService.GetResource("Common.NotDeactivable"));
            }

            if (ModelState.IsValid)
            {
                var assignment = requestForQuotation.Assignment;
                var assignmentHistories = _assignmentHistoryRepository.GetAll()
                    .Where(a => a.EntityId == requestForQuotation.Id && a.EntityType == EntityType.RequestForQuotation)
                    .ToList();

                _requestForQuotationRepository.Deactivate(requestForQuotation);
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
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Delete")]
        public ActionResult DeleteSelected(long? parentId, ICollection<long> selectedIds)
        {
            var requestForQuotations = new List<RequestForQuotation>();
            foreach (long id in selectedIds)
            {
                var requestForQuotation = _requestForQuotationRepository.GetById(id);
                if (requestForQuotation != null)
                {
                    if (!_requestForQuotationService.IsDeactivable(requestForQuotation))
                    {
                        ModelState.AddModelError("RequestForQuotation", _localizationService.GetResource("Common.NotDeactivable"));
                        break;
                    }
                    else
                    {
                        requestForQuotations.Add(requestForQuotation);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                foreach (var requestForQuotation in requestForQuotations)
                {
                    var assignment = requestForQuotation.Assignment;
                    var assignmentHistories = _assignmentHistoryRepository.GetAll()
                        .Where(a => a.EntityId == requestForQuotation.Id && a.EntityType == EntityType.RequestForQuotation)
                        .ToList();

                    _requestForQuotationRepository.Deactivate(requestForQuotation);
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

        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult Preview(long id)
        {
            var requestForQuotation = _requestForQuotationRepository.GetById(id);
            var model = requestForQuotation.MapTo<RequestForQuotation, RequestForQuotationPrintModel>();
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult SendToVendor(long requestForQuotationId)
        {
            var rfq = _requestForQuotationRepository.GetById(requestForQuotationId);

            var model = rfq.MapTo<RequestForQuotation, RequestForQuotationPrintModel>();

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
                Name = rfq.Number + ".pdf",
                FileBytes = fileBytes,
                FileSize = fileBytes.Length,
                Extension = ".pdf",
                ContentType = "application/pdf"
            };
            attachment.EntityAttachments.Add(new EntityAttachment { EntityId = rfq.Id, EntityType = EntityType.RequestForQuotation });
            _attachmentRepository.InsertAndCommit(attachment);

            // send to vendor
            foreach (var rfqVendor in rfq.RequestForQuotationVendors)
            {
                _messageService.SendEmail(
                rfqVendor.Vendor.Addresses.ElementAt(0).Email,
                "",
                "",
                "",
                "admin@baseeam.com",
                "Request For Quotation",
                "Please see RFQ's details in attachment.",
                attachment.Id.ToString());
            }

            rfq.IsSent = true;
            _requestForQuotationRepository.UpdateAndCommit(rfq);

            SuccessNotification(_localizationService.GetResource("PurchaseOrder.SentToVendor"));
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult CreatePO(long requestForQuotationVendorId)
        {
            var rfqVendor = _requestForQuotationVendorRepository.GetById(requestForQuotationVendorId);
            var po = _purchaseOrderRepository.GetAll()
                .Where(r => r.RequestForQuotationVendorId == requestForQuotationVendorId)
                .FirstOrDefault();
            if (po != null)
            {
                return Json(new { Errors = _localizationService.GetResource("PO.AlreadyCreatedForRFQVendor") });
            }
            // Validate the condition of RFQ vendor
            if (!rfqVendor.RequestForQuotationVendorItems.Any(r => r.IsAwarded == true))
            {
                return Json(new { Errors = _localizationService.GetResource("RequestForQuotation.AtLeastOneAwarded") });
            }

            try
            {
                _purchaseOrderService.CreatePO(rfqVendor);
            }
            catch (Exception e)
            {
                return Json(new { Errors = e.Message });
            }
            SuccessNotification(_localizationService.GetResource("PO.CreatedForRFQ"));
            return new NullJsonResult();
        }

        #endregion

        #region RequestForQuotationItem

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Read,Purchasing.RequestForQuotation.Update")]
        public ActionResult RequestForQuotationItemList(long requestForQuotationId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _requestForQuotationItemRepository.GetAll().Where(c => c.RequestForQuotationId == requestForQuotationId);
            query = sort == null ? query.OrderBy(a => a.Sequence) : query.Sort(sort);
            var requestForQuotationItems = new PagedList<RequestForQuotationItem>(query, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = requestForQuotationItems.Select(x => x.ToModel()),
                Total = requestForQuotationItems.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Read,Purchasing.RequestForQuotation.Update")]
        public ActionResult RequestForQuotationItem(long id)
        {
            var requestForQuotationItem = _requestForQuotationItemRepository.GetById(id);
            var model = requestForQuotationItem.ToModel();
            var html = this.RequestForQuotationItemPanel(model);
            return Json(new { Id = requestForQuotationItem.Id, Html = html });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult CreateRequestForQuotationItem(long requestForQuotationId)
        {
            //need to get requestForQuotation here to assign to new requestForQuotationItem
            var requestForQuotation = _requestForQuotationRepository.GetById(requestForQuotationId);
            var maxSequence = requestForQuotation.RequestForQuotationItems.Max(p => p.Sequence) ?? 0;
            var requestForQuotationItem = new RequestForQuotationItem
            {
                IsNew = true,
                RequestForQuotation = requestForQuotation,
                Sequence = maxSequence + 1
            };
            _requestForQuotationItemRepository.Insert(requestForQuotationItem);

            this._dbContext.SaveChanges();

            var model = new RequestForQuotationItemModel();
            model = requestForQuotationItem.ToModel();
            var html = this.RequestForQuotationItemPanel(model);

            return Json(new { Id = requestForQuotationItem.Id, Html = html });
        }

        [NonAction]
        public string RequestForQuotationItemPanel(RequestForQuotationItemModel model)
        {
            var html = this.RenderPartialViewToString("_LineItemDetails", model);
            return html;
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult SaveRequestForQuotationItem(RequestForQuotationItemModel model)
        {
            if (ModelState.IsValid)
            {
                var requestForQuotationItem = _requestForQuotationItemRepository.GetById(model.Id);
                //always set IsNew to false when saving
                requestForQuotationItem.IsNew = false;
                requestForQuotationItem = model.ToEntity(requestForQuotationItem);
                _requestForQuotationItemRepository.UpdateAndCommit(requestForQuotationItem);
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.Errors().ToHtmlString() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult CancelRequestForQuotationItem(long id)
        {
            var requestForQuotationItem = _requestForQuotationItemRepository.GetById(id);
            if (requestForQuotationItem.IsNew == true)
            {
                _requestForQuotationItemRepository.DeleteAndCommit(requestForQuotationItem);
            }
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult DeleteRequestForQuotationItem(long? parentId, long id)
        {
            var requestForQuotationItem = _requestForQuotationItemRepository.GetById(id);
            _requestForQuotationItemRepository.DeactivateAndCommit(requestForQuotationItem);
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult DeleteSelectedRequestForQuotationItems(long? parentId, long[] selectedIds)
        {
            foreach (long id in selectedIds)
            {
                var requestForQuotationItem = _requestForQuotationItemRepository.GetById(id);
                _requestForQuotationItemRepository.Deactivate(requestForQuotationItem);
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        #endregion

        #region Create RequestForQuotation Items

        [HttpGet]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult CreateRequestForQuotationItemsView()
        {
            var model = BuildCreateRequestForQuotationItemsSearchModel();
            return PartialView("_CreateLineItems", model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult CreateRequestForQuotationItemList(long requestForQuotationId, DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = BuildCreateRequestForQuotationItemsSearchModel();

            if (ModelState.IsValid)
            {
                model.Update(searchValues);

                PagedResult<Item> data = _itemService.GetItems(model.ToExpression(), model.ToParameters(), command.Page - 1, command.PageSize, sort);

                var gridModel = new DataSourceResult
                {
                    Data = data.Result.Select(x => new RequestForQuotationItemModel
                    {
                        RequestForQuotationId = requestForQuotationId,
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
        /// The list of creating requestForQuotation items is in updatedItems 
        /// </summary>
        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult CreateRequestForQuotationItems([Bind(Prefix = "updated")]List<RequestForQuotationItemModel> updatedItems,
           [Bind(Prefix = "created")]List<RequestForQuotationItemModel> createdItems,
           [Bind(Prefix = "deleted")]List<RequestForQuotationItemModel> deletedItems)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Create RequestForQuotationItems
                    if (updatedItems != null)
                    {
                        //get the current requestForQuotation
                        var requestForQuotationId = updatedItems.Count > 0 ? updatedItems[0].RequestForQuotationId : 0;
                        var requestForQuotationItems = _requestForQuotationItemRepository.GetAll().Where(r => r.RequestForQuotationId == requestForQuotationId).ToList();
                        var maxSequence = requestForQuotationItems.Max(p => p.Sequence) ?? 0;
                        for (int i = 1; i <= updatedItems.Count; i++)
                        {
                            var model = updatedItems[i - 1];
                            //we don't merge if the requestForQuotation item already existed
                            if (!requestForQuotationItems.Any(r => r.ItemId == model.ItemId))
                            {
                                //manually mapping here to set only foreign key
                                //if used AutoMapper, the reference property will also be mapped
                                //and EF will consider these properties as new and insert it
                                //so db records will be duplicated
                                //we can also ignore it in AutoMapper configuation instead of manually mapping
                                var requestForQuotationItem = new RequestForQuotationItem
                                {
                                    RequestForQuotationId = model.RequestForQuotationId,
                                    ItemId = model.ItemId,
                                    QuantityRequested = model.QuantityRequested,
                                    Sequence = maxSequence + i
                                };

                                _requestForQuotationItemRepository.Insert(requestForQuotationItem);
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

        #region Vendors

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Read")]
        public ActionResult VendorList(long requestForQuotationId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _requestForQuotationVendorRepository.GetAll().Where(c => c.RequestForQuotationId == requestForQuotationId);
            query = sort == null ? query.OrderBy(a => a.Name) : query.Sort(sort);
            var requestForQuotationVendors = new PagedList<RequestForQuotationVendor>(query, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = requestForQuotationVendors.Select(x => x.ToModel()),
                Total = requestForQuotationVendors.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult AddVendors(long requestForQuotationId, long[] selectedIds)
        {
            var requestForQuotation = _requestForQuotationRepository.GetById(requestForQuotationId);
            foreach (var id in selectedIds)
            {
                var existed = requestForQuotation.RequestForQuotationVendors.Any(s => s.VendorId == id);
                if (!existed)
                {
                    var vendor = _companyRepository.GetById(id);
                    var requestForQuotationVendor = new RequestForQuotationVendor
                    {
                        RequestForQuotationId = requestForQuotationId,
                        VendorId = id,
                        VendorContactName = vendor.Contacts.Count > 0 ? vendor.Contacts.ElementAt(0).Name : "",
                        VendorContactEmail = vendor.Contacts.Count > 0 ? vendor.Contacts.ElementAt(0).Email : "",
                        VendorContactPhone = vendor.Contacts.Count > 0 ? vendor.Contacts.ElementAt(0).Phone : "",
                        VendorContactFax = vendor.Contacts.Count > 0 ? vendor.Contacts.ElementAt(0).Fax : ""
                    };
                    _requestForQuotationVendorRepository.Insert(requestForQuotationVendor);
                    foreach(var rfqItem in requestForQuotation.RequestForQuotationItems)
                    {
                        requestForQuotationVendor.RequestForQuotationVendorItems.Add(new RequestForQuotationVendorItem
                        {
                            RequestForQuotationItemId = rfqItem.Id
                        });
                    }

                }
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult DeleteVendor(long? parentId, long id)
        {
            var requestForQuotationVendor = _requestForQuotationVendorRepository.GetById(id);
            _requestForQuotationVendorRepository.Deactivate(requestForQuotationVendor);
            foreach (var rfqVendorItem in requestForQuotationVendor.RequestForQuotationVendorItems)
                _requestForQuotationVendorItemRepository.Deactivate(rfqVendorItem);

            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult DeleteSelectedVendors(long? parentId, long[] selectedIds)
        {
            foreach (long id in selectedIds)
            {
                var requestForQuotationVendor = _requestForQuotationVendorRepository.GetById(id);
                _requestForQuotationVendorRepository.Deactivate(requestForQuotationVendor);
                foreach (var rfqVendorItem in requestForQuotationVendor.RequestForQuotationVendorItems)
                    _requestForQuotationVendorItemRepository.Deactivate(rfqVendorItem);
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        #endregion

        #region Quotations

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Read,Purchasing.RequestForQuotation.Update")]
        public ActionResult Quotation(long id)
        {
            var requestForQuotationVendor = _requestForQuotationVendorRepository.GetById(id);
            var model = requestForQuotationVendor.ToModel();
            var html = this.RequestForQuotationVendorPanel(model);
            return Json(new { Id = requestForQuotationVendor.Id, Html = html });
        }

        [NonAction]
        public string RequestForQuotationVendorPanel(RequestForQuotationVendorModel model)
        {
            var html = this.RenderPartialViewToString("_QuotationDetails", model);
            return html;
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult SaveQuotation(RequestForQuotationVendorModel model)
        {
            if (ModelState.IsValid)
            {
                var requestForQuotationVendor = _requestForQuotationVendorRepository.GetById(model.Id);
                //always set IsNew to false when saving
                requestForQuotationVendor.IsNew = false;
                requestForQuotationVendor = model.ToEntity(requestForQuotationVendor);
                _requestForQuotationVendorRepository.UpdateAndCommit(requestForQuotationVendor);
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.Errors().ToHtmlString() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult CancelQuotation(long id)
        {
            var requestForQuotationVendor = _requestForQuotationVendorRepository.GetById(id);
            if (requestForQuotationVendor.IsNew == true)
            {
                _requestForQuotationVendorRepository.DeleteAndCommit(requestForQuotationVendor);
            }
            return new NullJsonResult();
        }

        #endregion

        #region Quotation Items

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Read,Purchasing.RequestForQuotation.Update")]
        public ActionResult QuotationItemList(long requestForQuotationVendorId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            // sync the Line Items with Vendor Items
            bool needUpdate = false;
            var rfqVendor = _requestForQuotationVendorRepository.GetById(requestForQuotationVendorId);
            foreach (var rfqItem in rfqVendor.RequestForQuotation.RequestForQuotationItems)
            {
                if(!rfqVendor.RequestForQuotationVendorItems.Any(vi => vi.RequestForQuotationItemId == rfqItem.Id))
                {
                    rfqVendor.RequestForQuotationVendorItems.Add(new RequestForQuotationVendorItem
                    {
                        RequestForQuotationItemId = rfqItem.Id
                    });
                    needUpdate = true;
                }                
            }
            if(needUpdate == true)
            {
                this._dbContext.SaveChanges();
            }            

            var query = _requestForQuotationVendorItemRepository.GetAll().Where(c => c.RequestForQuotationVendorId == requestForQuotationVendorId);
            query = sort == null ? query.OrderBy(a => a.RequestForQuotationItem.Sequence) : query.Sort(sort);
            var quotationItems = new PagedList<RequestForQuotationVendorItem>(query, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = quotationItems.Select(x => x.ToModel()),
                Total = quotationItems.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Purchasing.RequestForQuotation.Create,Purchasing.RequestForQuotation.Update")]
        public ActionResult SaveChanges([Bind(Prefix = "updated")]List<RequestForQuotationVendorItemModel> updatedItems,
           [Bind(Prefix = "created")]List<RequestForQuotationVendorItemModel> createdItems,
           [Bind(Prefix = "deleted")]List<RequestForQuotationVendorItemModel> deletedItems)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (updatedItems != null)
                    {
                        foreach (var model in updatedItems)
                        {
                            var quotationItem = _requestForQuotationVendorItemRepository.GetById(model.Id);
                            quotationItem.QuantityQuoted = model.QuantityQuoted;
                            quotationItem.UnitPriceQuoted = model.UnitPriceQuoted;
                            quotationItem.SubtotalQuoted = (model.QuantityQuoted ?? 0) * (model.UnitPriceQuoted ?? 0);
                            quotationItem.IsAwarded = model.IsAwarded;
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