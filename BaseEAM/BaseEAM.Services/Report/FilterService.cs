﻿/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core;
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using BaseEAM.Data;
using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace BaseEAM.Services
{
    public class FilterService : BaseService, IFilterService
    {
        #region Fields

        private readonly IRepository<Filter> _filterRepository;
        private readonly DapperContext _dapperContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public FilterService(IRepository<Filter> filterRepository,
            DapperContext dapperContext,
            IDbContext dbContext)
        {
            this._filterRepository = filterRepository;
            this._dapperContext = dapperContext;
            this._dbContext = dbContext;
        }

        #endregion

        #region Methods

        public virtual PagedResult<Filter> GetFilters(string expression,
            dynamic parameters,
            int pageIndex = 0,
            int pageSize = 2147483647,
            IEnumerable<Core.Kendoui.Sort> sort = null)
        {
            var searchBuilder = new SqlBuilder();
            var search = searchBuilder.AddTemplate(SqlTemplate.FilterSearch(), new { skip = pageIndex * pageSize, take = pageSize });
            if (!string.IsNullOrEmpty(expression))
                searchBuilder.Where(expression, parameters);
            if (sort != null)
            {
                foreach (var s in sort)
                    searchBuilder.OrderBy(s.ToExpression());
            }
            else
            {
                searchBuilder.OrderBy("Name ASC");
            }

            var countBuilder = new SqlBuilder();
            var count = countBuilder.AddTemplate(SqlTemplate.FilterSearchCount());
            if (!string.IsNullOrEmpty(expression))
                countBuilder.Where(expression, parameters);

            using (var connection = _dapperContext.GetOpenConnection())
            {
                var filters = connection.Query<Filter>(search.RawSql, search.Parameters);
                var totalCount = connection.Query<int>(count.RawSql, search.Parameters).Single();
                return new PagedResult<Filter>(filters, totalCount);
            }
        }

        #endregion
    }
}
