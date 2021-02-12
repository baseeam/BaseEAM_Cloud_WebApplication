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
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Filters;
using BaseEAM.Core.Extensions;
using System;

namespace BaseEAM.Web.Controllers
{
    public class TenantPaymentController : BaseController
    {
        #region Fields
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<TenantPayment> _tenantPaymentRepository;
        private readonly IRepository<TenantPaymentCollection> _tenantPaymentCollectionRepository;
        private readonly ITenantPaymentService _tenantPaymentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly HttpContextBase _httpContext;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Constructors

        public TenantPaymentController(IRepository<TenantPayment> tenantPaymentRepository,
            IRepository<Tenant> tenantRepository,
            IRepository<TenantPaymentCollection> tenantPaymentCollectionRepository,
            ITenantPaymentService tenantPaymentService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            HttpContextBase httpContext,
            IWorkContext workContext,
            IDbContext dbContext)
        {
            this._tenantPaymentRepository = tenantPaymentRepository;
            this._tenantRepository = tenantRepository;
            this._tenantPaymentCollectionRepository = tenantPaymentCollectionRepository;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._tenantPaymentService = tenantPaymentService;
            this._permissionService = permissionService;
            this._httpContext = httpContext;
            this._workContext = workContext;
            this._dbContext = dbContext;
        }

        #endregion

        #region Utilities

        private SearchModel BuildSearchModel()
        {
            var model = new SearchModel();

            var siteFilter = new FieldModel
            {
                DisplayOrder = 1,
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

            var propertyFilter = new FieldModel
            {
                DisplayOrder = 2,
                Name = "Property",
                ResourceKey = "Property",
                DbColumn = "Property.Id",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int64,
                DataSource = FieldDataSource.MVC,
                MvcController = "Site",
                MvcAction = "PropertyList",
                ParentFieldName = "Site",
                IsRequiredField = false
            };
            model.Filters.Add(propertyFilter);

            var tenantFilter = new FieldModel
            {
                DisplayOrder = 3,
                Name = "Tenant",
                ResourceKey = "Tenant",
                DbColumn = "Tenant.Id",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.Int64,
                DataSource = FieldDataSource.DB,
                DbTable = "Tenant",
                DbTextColumn = "Name",
                DbValueColumn = "Id",
                IsRequiredField = false
            };
            model.Filters.Add(tenantFilter);

            var paymentDueFilter = new FieldModel
            {
                DisplayOrder = 4,
                Name = "DueDate",
                ResourceKey = "TenantPayment.DueDate",
                DbColumn = "TenantPayment.DueDate",
                Value = null,
                ControlType = FieldControlType.DropDownList,
                DataType = FieldDataType.String,
                DataSource = FieldDataSource.CSV,
                CsvTextList = "This Month,This Week,Last Month,Last Week,Last 90 Days,Last Year,Next 7 days,Next 14 days, Next 30 days, Next 60 days",
                CsvValueList = "ThisMonth,ThisWeek,LastMonth,LastWeek,Last90Days,LastYear,Next7days,Next14days,Next30days,Next60days",
                IsRequiredField = false
            };
            model.Filters.Add(paymentDueFilter);

            return model;
        }

        private string GenerateDueDateQuery(string paymentDueValue)
        {
            if (string.IsNullOrEmpty(paymentDueValue))
            {
                return "";
            }
            DateTime currentDate =  _dateTimeHelper.ConvertToUtcTime(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day));
            DateTime fromDate = currentDate;
            DateTime toDate = currentDate;
            string duaDateQuery = " AND TenantPayment.DueDate >='{0}' AND TenantPayment.DueDate <= '{1}'";
            switch (paymentDueValue)
            {
                case "ThisMonth":
                    fromDate = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0);
                    toDate = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month), 23, 59, 59);
                    break;
                case "ThisWeek":
                    fromDate = currentDate.AddDays(-1 * (int)(currentDate.DayOfWeek));
                    toDate = fromDate.AddDays(6);
                    //set timestamp
                    fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
                    break;
                case "LastMonth":
                    DateTime previousMonth = currentDate.AddMonths(-1);
                    fromDate = new DateTime(previousMonth.Year, previousMonth.Month, 1, 0, 0, 0);
                    toDate = new DateTime(previousMonth.Year, previousMonth.Month, DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month), 23, 59, 59);
                    break;
                case "LastWeek":
                    fromDate = currentDate.AddDays(-1 * (int)(currentDate.DayOfWeek) - 7);
                    toDate = fromDate.AddDays(6);
                    //set timestamp
                    fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
                    break;
                case "Last90Days":
                    fromDate = currentDate.AddDays(-90);
                    toDate = currentDate;
                    //set timestamp
                    fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
                    break;
                case "LastYear":
                    fromDate = currentDate.AddYears(-1);
                    toDate = fromDate;
                    //set timestamp
                    fromDate = new DateTime(fromDate.Year, 1, 1, 0, 0, 0);
                    toDate = new DateTime(toDate.Year, 12, DateTime.DaysInMonth(toDate.Year, toDate.Month), 23, 59, 59);
                    break;
                case "Next7days":
                    fromDate = currentDate;
                    toDate = currentDate.AddDays(7);
                    //set timestamp
                    fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
                    break;
                case "Next14days":
                    fromDate = currentDate;
                    toDate = currentDate.AddDays(14);
                    //set timestamp
                    fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
                    break;
                case "Next30days":
                    fromDate = currentDate;
                    toDate = currentDate.AddDays(30);
                    //set timestamp
                    fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
                    break;
                case "Next60days":
                    fromDate = currentDate;
                    toDate = currentDate.AddDays(60);
                    //set timestamp
                    fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
                    toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
                    break;
            }
            duaDateQuery = string.Format(duaDateQuery, fromDate.ToString("yyyy/MM/dd HH:mm:ss"), toDate.ToString("yyyy/MM/dd HH:mm:ss"));
            return duaDateQuery;
        }

        #endregion

        #region TenantPayments

        [BaseEamAuthorize(PermissionNames = "Property.TenantPayment.Read")]
        public ActionResult List()
        {
            var model = _httpContext.Session[SessionKey.TenantPaymentSearchModel] as SearchModel;
            //If not exist, build search model
            if (model == null)
            {
                model = BuildSearchModel();
                //session save
                _httpContext.Session[SessionKey.TenantPaymentSearchModel] = model;
            }
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantPayment.Read")]
        public ActionResult List(DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = _httpContext.Session[SessionKey.TenantPaymentSearchModel] as SearchModel;
            if (model == null)
                model = BuildSearchModel();
            else
                model.ClearValues();
            string searchValuesAfterRemovingPaymentDueValue = searchValues.Substring(0, searchValues.LastIndexOf("DueDate=") + "DueDate=".Length);
            //validate
            var errorFilters = model.Validate(searchValuesAfterRemovingPaymentDueValue);
            foreach (var filter in errorFilters)
            {
                ModelState.AddModelError(filter.Name, _localizationService.GetResource(filter.ResourceKey + ".Required"));
            }
            if (ModelState.IsValid)
            {
                //session update
                model.Update(searchValuesAfterRemovingPaymentDueValue);
                _httpContext.Session[SessionKey.TenantPaymentSearchModel] = model;
                //do a hack here to add duaDateQuery
                string paymentDueFilter = searchValues.Substring(searchValues.IndexOf("DueDate="));
                string paymentDueValue = paymentDueFilter.Split('=')[1];
                
                var expression = model.ToExpression(this._workContext.CurrentUser.Id);
                string duaDateQuery = GenerateDueDateQuery(paymentDueValue);
                expression = expression + duaDateQuery;
                PagedResult<TenantPayment> data = _tenantPaymentService.GetTenantPayments(expression, model.ToParameters(), command.Page - 1, command.PageSize, sort);

                var gridModel = new DataSourceResult
                {
                    Data = data.Result.Select(x => x.ToModel()),
                    Total = data.TotalCount
                };
                return new JsonResult
                {
                    Data = gridModel
                };
            }

            return Json(new { Errors = ModelState.SerializeErrors() });
        }
        #endregion

        #region TenantPaymentCollections

        [HttpPost]
        public ActionResult TenantPaymentCollections(long tenantPaymentId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _tenantPaymentCollectionRepository.GetAll().Where(f => f.TenantPaymentId == tenantPaymentId);
            query = sort == null ? query.OrderBy(a => a.Name) : query.Sort(sort);
            var tenantPaymentCollections = new PagedList<TenantPaymentCollection>(query, command.Page - 1, command.PageSize);
            var result = tenantPaymentCollections.Select(x => x.ToModel()).ToList();
            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = result.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.TenantPayment.Create,Property.TenantPayment.Update,Property.TenantPayment.Delete")]
        public ActionResult SaveChanges([Bind(Prefix = "updated")]List<TenantPaymentCollectionModel> updatedItems,
          [Bind(Prefix = "created")]List<TenantPaymentCollectionModel> createdItems,
          [Bind(Prefix = "deleted")]List<TenantPaymentCollectionModel> deletedItems)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Create TenantPaymentCollections
                    if (createdItems != null)
                    {
                        decimal? totalReceiveAmount = 0;
                        foreach (var model in createdItems)
                        {
                            var tenantPaymentCollection = new TenantPaymentCollection
                            {
                                TenantPaymentId = model.TenantPaymentId,
                                ReceivedDate = model.ReceivedDate,
                                ReceivedAmount = model.ReceivedAmount,
                                CheckNum = model.CheckNum,
                                PaymentMethodId = model.PaymentMethodId
                            };
                            totalReceiveAmount += model.ReceivedAmount;
                            _tenantPaymentCollectionRepository.Insert(tenantPaymentCollection);
                        }
                        long? tenantPaymentId = createdItems[0].TenantPaymentId;
                        var tenantPayment = _tenantPaymentRepository.GetById(tenantPaymentId);
                        tenantPayment.CollectedAmount =_tenantPaymentService.GetTotalReceiveAmount(tenantPayment) + totalReceiveAmount;
                        tenantPayment.BalanceAmount = tenantPayment.DueAmount + tenantPayment.LateFeeAmount - tenantPayment.CollectedAmount;
                        _tenantPaymentRepository.Update(tenantPayment);
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