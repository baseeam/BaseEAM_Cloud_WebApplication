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
    public class TenantPaymentService : BaseService, ITenantPaymentService
    {
        #region Fields

        private readonly IRepository<TenantPayment> _tenantPaymentRepository;
        private readonly IRepository<TenantPaymentCollection> _tenantPaymentCollectionRepository;
        private readonly DapperContext _dapperContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public TenantPaymentService(IRepository<TenantPayment> tenantPaymentRepository,
            IRepository<TenantPaymentCollection> tenantPaymentCollectionRepository,
            DapperContext dapperContext,
            IDbContext dbContext)
        {
            this._tenantPaymentRepository = tenantPaymentRepository;
            this._tenantPaymentCollectionRepository = tenantPaymentCollectionRepository;
            this._dapperContext = dapperContext;
            this._dbContext = dbContext;
        }

        #endregion

        #region Methods

        public virtual PagedResult<TenantPayment> GetTenantPayments(string expression,
            dynamic parameters,
            int pageIndex = 0,
            int pageSize = 2147483647,
            IEnumerable<Sort> sort = null)
        {
            var searchBuilder = new SqlBuilder();
            var search = searchBuilder.AddTemplate(SqlTemplate.TenantPaymentSearch(), new { skip = pageIndex * pageSize, take = pageSize });
            if (!string.IsNullOrEmpty(expression))
                searchBuilder.Where(expression, parameters);
            if (sort != null)
            {
                foreach (var s in sort)
                    searchBuilder.OrderBy(s.ToExpression());
            }
            else
            {
                searchBuilder.OrderBy("Tenant.Name");
            }

            var countBuilder = new SqlBuilder();
            var count = countBuilder.AddTemplate(SqlTemplate.TenantPaymentSearchCount());
            if (!string.IsNullOrEmpty(expression))
                countBuilder.Where(expression, parameters);

            using (var connection = _dapperContext.GetOpenConnection())
            {
                var tenantPayments = connection.Query<TenantPayment, Site, Tenant, Property, TenantLease, ValueItem, TenantPayment >(search.RawSql,
                    (tenantPayment, site, tenant, property, tenantLease, chargeType) => { tenantPayment.Site = site; tenantPayment.Tenant = tenant; tenantPayment.ChargeType = chargeType; tenantPayment.Property = property; tenantPayment.TenantLease = tenantLease; return tenantPayment; }, search.Parameters);
                var totalCount = connection.Query<int>(count.RawSql, search.Parameters).Single();
                return new PagedResult<TenantPayment>(tenantPayments, totalCount);
            }
        }

        public virtual decimal? GetTotalReceiveAmount(TenantPayment tenantPayment)
        {
            var totalReceiveAmount = _tenantPaymentCollectionRepository.GetAll().Where(p => p.TenantPaymentId == tenantPayment.Id).Sum(c => c.ReceivedAmount);
            return totalReceiveAmount ?? 0;
        }

        public virtual void DeletePayment(TenantPayment tenantPayment)
        {
            _tenantPaymentRepository.Delete(tenantPayment);
        }

        public virtual void UpdatePayment(TenantPayment tenantPayment)
        {
            _tenantPaymentRepository.Update(tenantPayment);
        }
        #endregion
    }
}
