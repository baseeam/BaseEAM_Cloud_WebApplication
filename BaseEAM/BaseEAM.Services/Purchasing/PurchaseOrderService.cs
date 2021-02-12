/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core;
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using BaseEAM.Core.Kendoui;
using BaseEAM.Data;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseEAM.Services
{
    public class PurchaseOrderService : BaseService, IPurchaseOrderService
    {
        #region Fields

        private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<RequestForQuotationVendorItem> _requestForQuotationVendorItemRepository;
        private readonly IAutoNumberService _autoNumberService;
        private readonly DapperContext _dapperContext;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public PurchaseOrderService(IRepository<PurchaseOrder> purchaseOrderRepository,
            IRepository<User> userRepository,
            IRepository<RequestForQuotationVendorItem> requestForQuotationVendorItemRepository,
            IAutoNumberService autoNumberService,
            DapperContext dapperContext,
            IWorkContext workContext,
            IDbContext dbContext,
            IDateTimeHelper dateTimeHelper)
        {
            this._purchaseOrderRepository = purchaseOrderRepository;
            this._userRepository = userRepository;
            this._requestForQuotationVendorItemRepository = requestForQuotationVendorItemRepository;
            this._autoNumberService = autoNumberService;
            this._dapperContext = dapperContext;
            this._workContext = workContext;
            this._dbContext = dbContext;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public virtual PagedResult<PurchaseOrder> GetPurchaseOrders(string expression,
            dynamic parameters,
            int pageIndex = 0,
            int pageSize = 2147483647,
            IEnumerable<Sort> sort = null)
        {
            var searchBuilder = new SqlBuilder();
            var search = searchBuilder.AddTemplate(SqlTemplate.PurchaseOrderSearch(), new { skip = pageIndex * pageSize, take = pageSize });
            if (!string.IsNullOrEmpty(expression))
                searchBuilder.Where(expression, parameters);
            if (sort != null)
            {
                foreach (var s in sort)
                    searchBuilder.OrderBy(s.ToExpression());
            }
            else
            {
                searchBuilder.OrderBy("PurchaseOrder.Number");
            }

            var countBuilder = new SqlBuilder();
            var count = countBuilder.AddTemplate(SqlTemplate.PurchaseOrderSearchCount());
            if (!string.IsNullOrEmpty(expression))
                countBuilder.Where(expression, parameters);

            using (var connection = _dapperContext.GetOpenConnection())
            {
                var purchaseOrders = connection.Query<PurchaseOrder, Company, ValueItem, Site, Assignment, PurchaseOrder>(search.RawSql,
                    (purchaseOrder, company, valueItem, site, assignment) => { purchaseOrder.Vendor = company; purchaseOrder.PaymentTerm = valueItem; purchaseOrder.Site = site; purchaseOrder.Assignment = assignment; return purchaseOrder; }, search.Parameters);
                var totalCount = connection.Query<int>(count.RawSql, search.Parameters).Single();
                return new PagedResult<PurchaseOrder>(purchaseOrders, totalCount);
            }
        }

        public virtual List<User> GetCreatedUser(long id)
        {
            var result = new List<User>();
            var purchaseOrder = _purchaseOrderRepository.GetById(id);
            var createdUser = _userRepository.GetById(purchaseOrder.CreatedUserId);
            result.Add(createdUser);
            return result;
        }

        public virtual PurchaseOrder CreatePO(PurchaseRequest pr)
        {
            var po = new PurchaseOrder
            {
                SiteId = pr.SiteId,
                PurchaseRequestId = pr.Id,
                RequestorId = pr.RequestorId,
                DateRequired = pr.DateRequired,
                Priority = pr.Priority,
                CreatedUserId = this._workContext.CurrentUser.Id
            };
            _purchaseOrderRepository.Insert(po);
            foreach (var prItem in pr.PurchaseRequestItems)
            {
                po.PurchaseOrderItems.Add(new PurchaseOrderItem
                {
                    Sequence = prItem.Sequence,
                    ItemId = prItem.ItemId,
                    QuantityOrdered = prItem.QuantityRequested
                });
            }

            string number = _autoNumberService.GenerateNextAutoNumber(_dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc), po);
            po.Number = number;

            this._dbContext.SaveChanges();
            //start workflow
            var workflowInstanceId = WorkflowServiceClient.StartWorkflow(po.Id, EntityType.PurchaseOrder, 0, this._workContext.CurrentUser.Id);
            this._dbContext.Detach(po);
            po = _purchaseOrderRepository.GetById(po.Id);
            this._dbContext.Detach(po.Assignment);
            // trigger Submit action
            WorkflowServiceClient.TriggerWorkflowAction(po.Id, EntityType.PurchaseOrder, po.Assignment.WorkflowDefinitionId, workflowInstanceId,
                        po.Assignment.WorkflowVersion.Value, WorkflowActionName.Submit, "", this._workContext.CurrentUser.Id);
            return po;
        }

        public virtual PurchaseOrder CreatePO(RequestForQuotationVendor rfqVendor)
        {
            var po = new PurchaseOrder
            {
                SiteId = rfqVendor.RequestForQuotation.SiteId,
                RequestForQuotationId = rfqVendor.RequestForQuotation.Id,
                RequestForQuotationVendorId = rfqVendor.Id,
                RequestorId = rfqVendor.RequestForQuotation.RequestorId,
                DateRequired = rfqVendor.RequestForQuotation.DateRequired,
                ShipToAddressId = rfqVendor.RequestForQuotation.ShipToAddressId,
                VendorId = rfqVendor.VendorId,
                Priority = rfqVendor.RequestForQuotation.Priority,
                CreatedUserId = this._workContext.CurrentUser.Id
            };
            _purchaseOrderRepository.Insert(po);
            var vendorItems = rfqVendor.RequestForQuotationVendorItems.Where(r => r.IsAwarded == true);
            foreach (var item in vendorItems)
            {
                po.PurchaseOrderItems.Add(new PurchaseOrderItem
                {
                    Sequence = item.RequestForQuotationItem.Sequence,
                    ItemId = item.RequestForQuotationItem.ItemId,
                    QuantityOrdered = item.QuantityQuoted,
                    UnitPrice = item.UnitPriceQuoted
                });
            }

            string number = _autoNumberService.GenerateNextAutoNumber(_dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc), po);
            po.Number = number;

            this._dbContext.SaveChanges();
            //start workflow
            var workflowInstanceId = WorkflowServiceClient.StartWorkflow(po.Id, EntityType.PurchaseOrder, 0, this._workContext.CurrentUser.Id);
            this._dbContext.Detach(po);
            po = _purchaseOrderRepository.GetById(po.Id);
            this._dbContext.Detach(po.Assignment);
            // trigger Submit action
            WorkflowServiceClient.TriggerWorkflowAction(po.Id, EntityType.PurchaseOrder, po.Assignment.WorkflowDefinitionId, workflowInstanceId,
                        po.Assignment.WorkflowVersion.Value, WorkflowActionName.Submit, "", this._workContext.CurrentUser.Id);
            return po;
        }

        #endregion    
    }
}
