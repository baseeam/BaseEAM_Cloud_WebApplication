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
    public class PropertyService : BaseService, IPropertyService
    {
        #region Fields

        private readonly IRepository<Property> _propertyRepository;
        private readonly DapperContext _dapperContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public PropertyService(IRepository<Property> propertyRepository,
            DapperContext dapperContext,
            IDbContext dbContext)
        {
            this._propertyRepository = propertyRepository;
            this._dapperContext = dapperContext;
            this._dbContext = dbContext;
        }

        #endregion

        #region Methods

        public virtual PagedResult<Property> GetProperties(string expression,
            dynamic parameters,
            int pageIndex = 0,
            int pageSize = 2147483647,
            IEnumerable<Sort> sort = null)
        {
            var searchBuilder = new SqlBuilder();
            var search = searchBuilder.AddTemplate(SqlTemplate.PropertySearch(), new { skip = pageIndex * pageSize, take = pageSize });
            if (!string.IsNullOrEmpty(expression))
                searchBuilder.Where(expression, parameters);
            if (sort != null)
            {
                foreach (var s in sort)
                    searchBuilder.OrderBy(s.ToExpression());
            }
            else
            {
                searchBuilder.OrderBy("Property.Name");
            }

            var countBuilder = new SqlBuilder();
            var count = countBuilder.AddTemplate(SqlTemplate.PropertySearchCount());
            if (!string.IsNullOrEmpty(expression))
                countBuilder.Where(expression, parameters);

            using (var connection = _dapperContext.GetOpenConnection())
            {
                var properties = connection.Query<Property, Site, Location, Property>(search.RawSql, (property, site, location) => { property.Site = site; property.Location = location; return property; }, search.Parameters);
                var totalCount = connection.Query<int>(count.RawSql, search.Parameters).Single();
                return new PagedResult<Property>(properties, totalCount);
            }
        }

        #endregion
    }
}
