/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Data;
using BaseEAM.Core.Domain;
using BaseEAM.Data;
using Common.Logging;
using Quartz;
using System;
using System.Collections;

namespace BaseEAM.Services
{
    [DisallowConcurrentExecution]
    public class ReorderItemsJob : IJob
    {
        private static readonly ILog s_log = LogManager.GetLogger<ReorderItemsJob>();
        private readonly IRepository<PurchaseRequest> _purchaseRequestRepository;
        private readonly IStoreService _storeService;
        private readonly IDbContext _dbContext;

        public ReorderItemsJob(IStoreService storeService,
            IRepository<PurchaseRequest> purchaseRequestRepository,
            IDbContext dbContext)
        {
            this._storeService = storeService;
            this._purchaseRequestRepository = purchaseRequestRepository;
            this._dbContext = dbContext;
        }

        /// <summary>
        /// Generate a new PR for each Store
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            s_log.Info("ReorderItemsJob START");

            var mapping = new Hashtable();
            int maxSequence = 0;
            var lowInventoryStoreItems = _storeService.GetLowInventoryStoreItems();
            foreach(var item in lowInventoryStoreItems.Result)
            {
                var pr = mapping[item.SiteId + "-" + item.StoreId] as PurchaseRequest;
                if (pr == null)
                {
                    maxSequence = 0;
                    pr = new PurchaseRequest
                    {
                        SiteId = item.SiteId,
                        DateRequired = DateTime.UtcNow.AddDays(7),
                        Description = "PR generated for low inventory items."
                    };
                    _purchaseRequestRepository.Insert(pr);
                    pr.PurchaseRequestItems.Add(new PurchaseRequestItem
                    {
                        Sequence = maxSequence + 1,
                        ItemId = item.ItemId,
                        QuantityRequested = CalculateQuantityRequested(item)
                    });

                    mapping.Add(item.SiteId + " - " + item.StoreId, pr);
                }
                else
                {
                    maxSequence = pr.PurchaseRequestItems.Count;
                    pr.PurchaseRequestItems.Add(new PurchaseRequestItem
                    {
                        Sequence = maxSequence + 1,
                        ItemId = item.ItemId,
                        QuantityRequested = CalculateQuantityRequested(item)
                    });
                }
            }

            this._dbContext.SaveChanges();

            s_log.Info("ReorderItemsJob END");
        }

        private decimal? CalculateQuantityRequested(LowInventoryStoreItem item)
        {
            decimal? quantity;
            if (item.EconomicOrderQuantity > 0)
            {
                quantity = Math.Ceiling((item.ReorderPoint.Value + 1 - item.TotalQuantity.Value) / item.EconomicOrderQuantity.Value) * item.EconomicOrderQuantity;
            }
            else
            {
                quantity = item.ReorderPoint + 1 - item.TotalQuantity;
            }

            return quantity;
        }
    }
}
