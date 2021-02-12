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
    public class RequestForQuotationService : BaseService, IRequestForQuotationService
    {
        #region Fields

        private readonly IRepository<RequestForQuotation> _requestForQuotationRepository;
        private readonly IRepository<User> _userRepository;
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
        public RequestForQuotationService(IRepository<RequestForQuotation> requestForQuotationRepository,
            IRepository<User> userRepository,
            IAutoNumberService autoNumberService,
            DapperContext dapperContext,
            IWorkContext workContext,
            IDbContext dbContext,
            IDateTimeHelper dateTimeHelper)
        {
            this._requestForQuotationRepository = requestForQuotationRepository;
            this._userRepository = userRepository;
            this._autoNumberService = autoNumberService;
            this._dapperContext = dapperContext;
            this._workContext = workContext;
            this._dbContext = dbContext;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public virtual PagedResult<RequestForQuotation> GetRequestForQuotations(string expression,
            dynamic parameters,
            int pageIndex = 0,
            int pageSize = 2147483647,
            IEnumerable<Sort> sort = null)
        {
            var searchBuilder = new SqlBuilder();
            var search = searchBuilder.AddTemplate(SqlTemplate.RequestForQuotationSearch(), new { skip = pageIndex * pageSize, take = pageSize });
            if (!string.IsNullOrEmpty(expression))
                searchBuilder.Where(expression, parameters);
            if (sort != null)
            {
                foreach (var s in sort)
                    searchBuilder.OrderBy(s.ToExpression());
            }
            else
            {
                searchBuilder.OrderBy("RequestForQuotation.Number");
            }

            var countBuilder = new SqlBuilder();
            var count = countBuilder.AddTemplate(SqlTemplate.RequestForQuotationSearchCount());
            if (!string.IsNullOrEmpty(expression))
                countBuilder.Where(expression, parameters);

            using (var connection = _dapperContext.GetOpenConnection())
            {
                var requestForQuotations = connection.Query<RequestForQuotation, Site, Assignment, RequestForQuotation>(search.RawSql,
                    (requestForQuotation, site, assignment) => { requestForQuotation.Site = site; requestForQuotation.Assignment = assignment; return requestForQuotation; }, search.Parameters);
                var totalCount = connection.Query<int>(count.RawSql, search.Parameters).Single();
                return new PagedResult<RequestForQuotation>(requestForQuotations, totalCount);
            }
        }

        public virtual List<User> GetCreatedUser(long id)
        {
            var result = new List<User>();
            var requestForQuotation = _requestForQuotationRepository.GetById(id);
            var createdUser = _userRepository.GetById(requestForQuotation.CreatedUserId);
            result.Add(createdUser);
            return result;
        }

        public virtual RequestForQuotation CreateRFQ(PurchaseRequest pr)
        {
            var rfq = new RequestForQuotation
            {
                SiteId = pr.SiteId,
                PurchaseRequestId = pr.Id,
                RequestorId = pr.RequestorId,
                DateRequired = pr.DateRequired,
                Priority = pr.Priority,
                CreatedUserId = this._workContext.CurrentUser.Id
            };
            _requestForQuotationRepository.Insert(rfq);
            foreach(var prItem in pr.PurchaseRequestItems)
            {
                rfq.RequestForQuotationItems.Add(new RequestForQuotationItem
                {
                    Sequence = prItem.Sequence,
                    ItemId = prItem.ItemId,
                    QuantityRequested = prItem.QuantityRequested
                });
            }

            string number = _autoNumberService.GenerateNextAutoNumber(_dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, DateTimeKind.Utc), rfq);
            rfq.Number = number;

            this._dbContext.SaveChanges();
            //start workflow
            var workflowInstanceId = WorkflowServiceClient.StartWorkflow(rfq.Id, EntityType.RequestForQuotation, 0, this._workContext.CurrentUser.Id);
            this._dbContext.Detach(rfq);
            rfq = _requestForQuotationRepository.GetById(rfq.Id);
            this._dbContext.Detach(rfq.Assignment);
            // trigger Submit action
            WorkflowServiceClient.TriggerWorkflowAction(rfq.Id, EntityType.RequestForQuotation, rfq.Assignment.WorkflowDefinitionId, workflowInstanceId,
                        rfq.Assignment.WorkflowVersion.Value, WorkflowActionName.Submit, "", this._workContext.CurrentUser.Id);
            return rfq;
        }

        #endregion    
    }
}
