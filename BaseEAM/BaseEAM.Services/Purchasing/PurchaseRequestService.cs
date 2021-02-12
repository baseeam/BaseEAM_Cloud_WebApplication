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
using System.Collections.Generic;
using System.Linq;

namespace BaseEAM.Services
{
    public class PurchaseRequestService : BaseService, IPurchaseRequestService
    {
        #region Fields

        private readonly IRepository<PurchaseRequest> _purchaseRequestRepository;
        private readonly IRepository<User> _userRepository;
        private readonly DapperContext _dapperContext;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public PurchaseRequestService(IRepository<PurchaseRequest> purchaseRequestRepository,
            IRepository<User> userRepository,
            DapperContext dapperContext,
            IWorkContext workContext,
            IDbContext dbContext)
        {
            this._purchaseRequestRepository = purchaseRequestRepository;
            this._userRepository = userRepository;
            this._dapperContext = dapperContext;
            this._workContext = workContext;
            this._dbContext = dbContext;
        }

        #endregion

        #region Methods

        public virtual PagedResult<PurchaseRequest> GetPurchaseRequests(string expression,
            dynamic parameters,
            int pageIndex = 0,
            int pageSize = 2147483647,
            IEnumerable<Sort> sort = null)
        {
            var searchBuilder = new SqlBuilder();
            var search = searchBuilder.AddTemplate(SqlTemplate.PurchaseRequestSearch(), new { skip = pageIndex * pageSize, take = pageSize });
            if (!string.IsNullOrEmpty(expression))
                searchBuilder.Where(expression, parameters);
            if (sort != null)
            {
                foreach (var s in sort)
                    searchBuilder.OrderBy(s.ToExpression());
            }
            else
            {
                searchBuilder.OrderBy("PurchaseRequest.Number");
            }

            var countBuilder = new SqlBuilder();
            var count = countBuilder.AddTemplate(SqlTemplate.PurchaseRequestSearchCount());
            if (!string.IsNullOrEmpty(expression))
                countBuilder.Where(expression, parameters);

            using (var connection = _dapperContext.GetOpenConnection())
            {
                var purchaseRequests = connection.Query<PurchaseRequest, Site, Assignment, PurchaseRequest>(search.RawSql,
                    (purchaseRequest, site, assignment) => { purchaseRequest.Site = site; purchaseRequest.Assignment = assignment; return purchaseRequest; }, search.Parameters);
                var totalCount = connection.Query<int>(count.RawSql, search.Parameters).Single();
                return new PagedResult<PurchaseRequest>(purchaseRequests, totalCount);
            }
        }

        public virtual List<User> GetCreatedUser(long id)
        {
            var result = new List<User>();
            var purchaseRequest = _purchaseRequestRepository.GetById(id);
            var createdUser = _userRepository.GetById(purchaseRequest.CreatedUserId);
            result.Add(createdUser);
            return result;
        }

        #endregion    
    }
}
