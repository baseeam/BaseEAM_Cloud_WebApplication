﻿/*******************************************************
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
    public class VisualService : BaseService, IVisualService
    {
        #region Fields

        private readonly IRepository<Visual> _visualRepository;
        private readonly DapperContext _dapperContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public VisualService(IRepository<Visual> visualRepository,
            DapperContext dapperContext,
            IDbContext dbContext)
        {
            this._visualRepository = visualRepository;
            this._dapperContext = dapperContext;
            this._dbContext = dbContext;
        }

        #endregion

        #region Methods

        public virtual PagedResult<Visual> GetVisuals(string expression,
            dynamic parameters,
            int pageIndex = 0,
            int pageSize = 2147483647,
            IEnumerable<Sort> sort = null)
        {
            var searchBuilder = new SqlBuilder();
            var search = searchBuilder.AddTemplate(SqlTemplate.VisualSearch(), new { skip = pageIndex * pageSize, take = pageSize });
            if (!string.IsNullOrEmpty(expression))
                searchBuilder.Where(expression, parameters);
            if (sort != null)
            {
                foreach (var s in sort)
                    searchBuilder.OrderBy(s.ToExpression());
            }
            else
            {
                searchBuilder.OrderBy("Name");
            }

            var countBuilder = new SqlBuilder();
            var count = countBuilder.AddTemplate(SqlTemplate.VisualSearchCount());
            if (!string.IsNullOrEmpty(expression))
                countBuilder.Where(expression, parameters);

            using (var connection = _dapperContext.GetOpenConnection())
            {
                var visuals = connection.Query<Visual>(search.RawSql, search.Parameters);
                var totalCount = connection.Query<int>(count.RawSql, search.Parameters).Single();
                return new PagedResult<Visual>(visuals, totalCount);
            }
        }

        public virtual List<Visual> GetVisualsByUser(User user)
        {
            var result = new List<Visual>();
            var securityGroupIds = user.SecurityGroups.Select(g => g.Id).ToList();
            result = _visualRepository.GetAll()
                .Where(r => r.SecurityGroups.Any(g => securityGroupIds.Contains(g.Id)))
                .OrderBy(r => r.Name)
                .ToList();
            return result;
        }

        public virtual IEnumerable<dynamic> GetVisualData(Visual visual,
            string expression,
            dynamic parameters,
            IEnumerable<Sort> sort = null)
        {
            var searchBuilder = new SqlBuilder();
            var search = searchBuilder.AddTemplate(visual.Query);
            if (!string.IsNullOrEmpty(expression))
                searchBuilder.Where(expression, parameters);
            if (sort != null)
            {
                foreach (var s in sort)
                    searchBuilder.OrderBy(s.ToExpression());
            }
            else
            {
                searchBuilder.OrderBy(visual.SortExpression);
            }

            using (var connection = _dapperContext.GetOpenConnection())
            {
                var result = connection.Query(search.RawSql, search.Parameters);
                return result;
            }
        }

        #endregion
    }
}
