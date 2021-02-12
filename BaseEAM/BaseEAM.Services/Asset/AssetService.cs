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
    public class AssetService : BaseService, IAssetService
    {
        #region Fields

        private readonly IRepository<Asset> _assetRepository;
        private readonly IDapperRepository<Asset> _assetDapperRepository;
        private readonly DapperContext _dapperContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public AssetService(IRepository<Asset> assetRepository,
            IDapperRepository<Asset> assetDapperRepository,
            DapperContext dapperContext,
            IDbContext dbContext)
        {
            this._assetRepository = assetRepository;
            this._assetDapperRepository = assetDapperRepository;
            this._dapperContext = dapperContext;
            this._dbContext = dbContext;
        }

        #endregion

        #region Methods

        public virtual PagedResult<Asset> GetAssets(string expression,
            dynamic parameters,
            int pageIndex = 0,
            int pageSize = 2147483647,
            IEnumerable<Sort> sort = null)
        {
            var searchBuilder = new SqlBuilder();
            var search = searchBuilder.AddTemplate(SqlTemplate.AssetSearch(), new { skip = pageIndex * pageSize, take = pageSize });
            if (!string.IsNullOrEmpty(expression))
                searchBuilder.Where(expression, parameters);
            if (sort != null)
            {
                foreach (var s in sort)
                    searchBuilder.OrderBy(s.ToExpression());
            }
            else
            {
                searchBuilder.OrderBy("Asset.Name");
            }

            var countBuilder = new SqlBuilder();
            var count = countBuilder.AddTemplate(SqlTemplate.AssetSearchCount());
            if (!string.IsNullOrEmpty(expression))
                countBuilder.Where(expression, parameters);

            using (var connection = _dapperContext.GetOpenConnection())
            {
                var assets = connection.Query<Asset, ValueItem, ValueItem, ValueItem, Location, Site, Asset>(search.RawSql,
                    (asset, assetType, assetStatus, assetCategory, location, site) => { asset.AssetType = assetType; asset.AssetStatus = assetStatus; asset.AssetCategory = assetCategory; asset.Location = location;  asset.Site = site; return asset; }, search.Parameters);
                var totalCount = connection.Query<int>(count.RawSql, search.Parameters).Single();
                return new PagedResult<Asset>(assets, totalCount);
            }
        }

        public virtual List<Asset> GetAllAssetsByParentId(long? parentId)
        {
            var assets = _assetRepository.GetAll().Where(c => c.ParentId == parentId).OrderBy(c => c.Name).ToList();
            return assets;
        }

        public virtual void CalculateDepreciation()
        {
            // Load all assets with No Depreciation == false
            var assets = _assetRepository.GetAll()
                .Where(a => a.NoDepreciation == false)
                .ToList();
            foreach(var asset in assets)
            {
                if (IsValidDepreciationParams(asset))
                {
                    CalculateAssetDepreciation(asset);
                    _assetDapperRepository.Update(asset);
                }
            }
        }

        private bool IsValidDepreciationParams(Asset asset)
        {
            bool isValid = true;
            if (asset.DepreciationStartDate == null)
            {
                isValid = false;
            }
            else if (asset.DepreciationLifeSpan == null || asset.DepreciationLifeSpan == 0)
            {
                isValid = false;
            }
            else if (asset.DepreciationOriginalValue == null || asset.DepreciationOriginalValue == 0)
            {
                isValid = false;
            }
            else if (asset.DepreciationEndValue == null || asset.DepreciationEndValue == 0)
            {
                isValid = false;
            }
            else if (asset.DepreciationOriginalValue <= asset.DepreciationEndValue)
            {
                isValid = false;
            }
            return isValid;
        }

        private void CalculateAssetDepreciation(Asset asset)
        {
            int numberOfPeriodsElapsed = CalculatePeriodsElapsed(asset);
            if (numberOfPeriodsElapsed == asset.DepreciationPeriodCount)
            {
                asset.DepreciationCalculatedDateTime = DateTime.UtcNow;
            }
            else
            {
                asset.DepreciationPeriodCount = numberOfPeriodsElapsed;
                asset.DepreciationCalculatedDateTime = DateTime.UtcNow;
                decimal numberOfPeriods = CalculatePeriods(asset);
                decimal depreciateValue = 0;
                if (asset.DepreciationType == (int?)DepreciationType.StraightLine)
                {
                    depreciateValue = 
                        (asset.DepreciationOriginalValue.Value - asset.DepreciationEndValue.Value) / numberOfPeriods;                    
                }
                else if (asset.DepreciationType == (int?)DepreciationType.DoubleDeclining)
                {
                    depreciateValue =
                        (asset.DepreciationOriginalValue.Value - (asset.AccumulatedDepreciation ?? 0)) * 2 / numberOfPeriods;
                }
                if (asset.DepreciationPeriodCount == 0)
                {
                    asset.AccumulatedDepreciation = 0;
                }
                else
                {
                    asset.AccumulatedDepreciation = (asset.AccumulatedDepreciation ?? 0) + depreciateValue;
                }
                asset.CurrentPeriodDepreciation = depreciateValue;
                asset.UndepreciatedBalance =
                    asset.DepreciationOriginalValue - asset.DepreciationEndValue - asset.AccumulatedDepreciation - asset.CurrentPeriodDepreciation;
            }
        }

        private int CalculatePeriodsElapsed(Asset asset)
        {
            int numberOfPeriodsElapsed = 0;
            int numberOfDaysElapsed = (DateTime.UtcNow - asset.DepreciationStartDate).Value.Days;
            if (asset.DepreciationPeriodType == (int?)DepreciationPeriodType.Monthly)
            {
                numberOfPeriodsElapsed = numberOfDaysElapsed / 30;
            }
            else if (asset.DepreciationPeriodType == (int?)DepreciationPeriodType.Quarterly)
            {
                numberOfPeriodsElapsed = numberOfDaysElapsed / 90;
            }
            else if (asset.DepreciationPeriodType == (int?)DepreciationPeriodType.Semiyearly)
            {
                numberOfPeriodsElapsed = numberOfDaysElapsed / 180;
            }
            else if (asset.DepreciationPeriodType == (int?)DepreciationPeriodType.Yearly)
            {
                numberOfPeriodsElapsed = numberOfDaysElapsed / 365;
            }
            return numberOfPeriodsElapsed;
        }

        private decimal CalculatePeriods(Asset asset)
        {
            decimal numberOfPeriods = 0;
            if (asset.DepreciationPeriodType == (int?)DepreciationPeriodType.Monthly)
            {
                numberOfPeriods = asset.DepreciationLifeSpan.Value * 12;
            }
            else if (asset.DepreciationPeriodType == (int?)DepreciationPeriodType.Quarterly)
            {
                numberOfPeriods = asset.DepreciationLifeSpan.Value * 4;
            }
            else if (asset.DepreciationPeriodType == (int?)DepreciationPeriodType.Semiyearly)
            {
                numberOfPeriods = asset.DepreciationLifeSpan.Value * 2;
            }
            else if (asset.DepreciationPeriodType == (int?)DepreciationPeriodType.Yearly)
            {
                numberOfPeriods = asset.DepreciationLifeSpan.Value;
            }
            return numberOfPeriods;
        }

        #endregion
    }
}
