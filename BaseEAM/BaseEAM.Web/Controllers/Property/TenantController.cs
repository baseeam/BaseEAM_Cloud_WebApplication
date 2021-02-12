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

namespace BaseEAM.Web.Controllers
{
    public class TenantController : BaseController
    {
        #region Fields
        private readonly string[] daysOfMonth = new string[] {
                "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th", "13th", "14th", "15th",
                "16th", "17th", "18th", "19th", "20th", "21st", "22nd", "23rd", "24th", "25th", "26th", "27th", "28th", "29th*", "30th*", "31st*"
        };

        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<TenantLease> _tenantLeaseRepository;
        private readonly IRepository<TenantPayment> _tenantPaymentRepository;
        private readonly IRepository<Contact> _contactRepository;
        private readonly ITenantService _tenantService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly HttpContextBase _httpContext;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Constructors

        public TenantController(IRepository<Tenant> tenantRepository,
            IRepository<Contact> contactRepository,
            IRepository<TenantLease> tenantLeaseRepository,
            IRepository<TenantPayment> tenantPaymentRepository,
            ITenantService tenantService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            HttpContextBase httpContext,
            IWorkContext workContext,
            IDbContext dbContext)
        {
            this._tenantRepository = tenantRepository;
            this._contactRepository = contactRepository;
            this._tenantLeaseRepository = tenantLeaseRepository;
            this._tenantPaymentRepository = tenantPaymentRepository;
            this._localizationService = localizationService;
            this._tenantService = tenantService;
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
            var tenantNameFilter = new FieldModel
            {
                DisplayOrder = 1,
                Name = "Name",
                ResourceKey = "Common.Name",
                DbColumn = "Name",
                Value = null,
                ControlType = FieldControlType.TextBox,
                DataType = FieldDataType.String,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(tenantNameFilter);

            return model;
        }

        [NonAction]
        protected virtual void PrepareTenantLeaseModel(TenantLeaseModel model)
        {
            //Term Numbers
            for (int i = 1; i <= 60; i++)
            {
                model.AvailableTermNumbers.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString(),
                    Selected = model.TermNumber == i
                });
            }

            //BiMonthly Starts
            for (int i = 1; i <= daysOfMonth.Length; i++)
            {
                model.AvailableBiMonthlyStarts.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = daysOfMonth[i - 1].ToString(),
                    Selected = model.BiMonthlyStart == i
                });
            }
            //BiMonthly Ends
            for (int i = 1; i <= daysOfMonth.Length; i++)
            {
                model.AvailableBiMonthlyEnds.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = daysOfMonth[i - 1].ToString(),
                    Selected = model.BiMonthlyEnd == i
                });
            }
        }
        #endregion

        #region Tenants

        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Read")]
        public ActionResult List()
        {
            var model = _httpContext.Session[SessionKey.TenantSearchModel] as SearchModel;
            //If not exist, build search model
            if (model == null)
            {
                model = BuildSearchModel();
                //session save
                _httpContext.Session[SessionKey.TenantSearchModel] = model;
            }
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Read")]
        public ActionResult List(DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = _httpContext.Session[SessionKey.TenantSearchModel] as SearchModel;
            if (model == null)
                model = BuildSearchModel();
            else
                model.ClearValues();
            //validate
            var errorFilters = model.Validate(searchValues);
            foreach (var filter in errorFilters)
            {
                ModelState.AddModelError(filter.Name, _localizationService.GetResource(filter.ResourceKey + ".Required"));
            }
            if (ModelState.IsValid)
            {
                //session update
                model.Update(searchValues);
                _httpContext.Session[SessionKey.TenantSearchModel] = model;

                PagedResult<Tenant> data = _tenantService.GetTenants(model.ToExpression(), model.ToParameters(), command.Page - 1, command.PageSize, sort);

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

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Create")]
        public ActionResult Create()
        {
            var tenant = new Tenant { IsNew = true };
            _tenantRepository.InsertAndCommit(tenant);
            return Json(new { Id = tenant.Id });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Create")]
        public ActionResult Cancel(long? parentId, long id)
        {
            this._dbContext.DeleteById<Tenant>(id);
            return new NullJsonResult();
        }

        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Create,Property.Tenant.Read,Property.Tenant.Update")]
        public ActionResult Edit(long id)
        {
            var tenant = _tenantRepository.GetById(id);
            var model = tenant.ToModel();
            if (model.Address == null)
                model.Address = new AddressModel();

            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Create,Property.Tenant.Update")]
        public ActionResult Edit(TenantModel model)
        {
            var tenant = _tenantRepository.GetById(model.Id);
            if (ModelState.IsValid)
            {
                tenant = model.ToEntity(tenant);
                //set id for Address
                if (tenant.AddressId > 0)
                {
                    tenant.Address.Id = tenant.AddressId.Value;
                }

                //always set IsNew to false when saving
                tenant.IsNew = false;
                _tenantRepository.Update(tenant);

                //commit all changes
                this._dbContext.SaveChanges();

                //notification
                SuccessNotification(_localizationService.GetResource("Record.Saved"));
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Delete")]
        public ActionResult Delete(long? parentId, long id)
        {
            var tenant = _tenantRepository.GetById(id);

            if (!_tenantService.IsDeactivable(tenant))
            {
                ModelState.AddModelError("Tenant", _localizationService.GetResource("Common.NotDeactivable"));
            }

            if (ModelState.IsValid)
            {
                _tenantRepository.DeactivateAndCommit(tenant);
                //notification
                SuccessNotification(_localizationService.GetResource("Record.Deleted"));
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Delete")]
        public ActionResult DeleteSelected(long? parentId, ICollection<long> selectedIds)
        {
            var tenants = new List<Tenant>();
            foreach (long id in selectedIds)
            {
                var tenant = _tenantRepository.GetById(id);
                if (tenant != null)
                {
                    if (!_tenantService.IsDeactivable(tenant))
                    {
                        ModelState.AddModelError("Tenant", _localizationService.GetResource("Common.NotDeactivable"));
                        break;
                    }
                    else
                    {
                        tenants.Add(tenant);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                foreach (var tenant in tenants)
                    _tenantRepository.Deactivate(tenant);
                this._dbContext.SaveChanges();
                SuccessNotification(_localizationService.GetResource("Record.Deleted"));
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }
        }

        #endregion

        #region Contacts

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Read,Property.Contact.Read,Property.Tenant.Portal")]
        public ActionResult ContactList(long tenantId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _contactRepository.GetAll().Where(c => c.TenantId == tenantId);
            query = sort == null ? query.OrderBy(a => a.Name) : query.Sort(sort);
            var contacts = new PagedList<Contact>(query, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = contacts.Select(x => x.ToModel()),
                Total = contacts.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Read")]
        public ActionResult Contact(long id)
        {
            var contact = _contactRepository.GetById(id);
            var model = contact.ToModel();
            var html = this.ContactPanel(model);
            return Json(new { Id = contact.Id, Html = html });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Create")]
        public ActionResult CreateContact(long tenantId)
        {
            var contact = new Contact
            {
                IsNew = true
            };
            _contactRepository.Insert(contact);

            var tenant = _tenantRepository.GetById(tenantId);
            tenant.Contacts.Add(contact);

            this._dbContext.SaveChanges();

            var model = new ContactModel();
            model = contact.ToModel();
            var html = this.ContactPanel(model);

            return Json(new { Id = contact.Id, Html = html });
        }

        [NonAction]
        public string ContactPanel(ContactModel model)
        {
            var html = this.RenderPartialViewToString("_ContactDetails", model);
            return html;
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Create,Property.Tenant.Update")]
        public ActionResult SaveContact(ContactModel model)
        {
            if (ModelState.IsValid)
            {
                var contact = _contactRepository.GetById(model.Id);
                //always set IsNew to false when saving
                contact.IsNew = false;
                contact = model.ToEntity(contact);

                _contactRepository.UpdateAndCommit(contact);
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.Errors().ToHtmlString() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Create,Property.Tenant.Update")]
        public ActionResult CancelContact(long id)
        {
            var contact = _contactRepository.GetById(id);
            if (contact.IsNew == true)
            {
                _contactRepository.DeleteAndCommit(contact);
            }
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Delete")]
        public ActionResult DeleteContact(long? parentId, long id)
        {
            var contact = _contactRepository.GetById(id);
            //For parent-child, we can mark deleted to child
            _contactRepository.DeactivateAndCommit(contact);
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Delete")]
        public ActionResult DeleteSelectedContacts(long? parentId, long[] selectedIds)
        {
            foreach (long id in selectedIds)
            {
                var contact = _contactRepository.GetById(id);
                //For parent-child, we can mark deleted to child
                _contactRepository.Deactivate(contact);
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        #endregion

        #region TenantLeases

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Read,Property.TenantLease.Read,Property.Tenant.Portal")]
        public ActionResult TenantLeaseList(long tenantId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _tenantLeaseRepository.GetAll().Where(c => c.TenantId == tenantId);
            query = sort == null ? query.OrderBy(a => a.Number) : query.Sort(sort);
            var data = new PagedList<TenantLease>(query, command.Page - 1, command.PageSize);

            var result = data.Select(x => x.ToModel()).ToList();
            foreach (var item in result)
            {
                item.PriorityText = item.Priority.ToString();
            }

            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = data.TotalCount
            };

            return Json(gridModel);
        }

        #endregion

        #region Portal

        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Portal")]
        public ActionResult Portal()
        {
            var tenant = _tenantRepository.GetAll()
                .Where(t => t.UserId == _workContext.CurrentUser.Id)
                .FirstOrDefault();
            var model = tenant.ToModel();
            if (model.Address == null)
                model.Address = new AddressModel();

            return View(model);
        }


        #endregion

        #region TenantLeases

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Portal")]
        public ActionResult TenantLease(long id)
        {
            var tenantLease = _tenantLeaseRepository.GetById(id);
            var model = tenantLease.ToModel();
            PrepareTenantLeaseModel(model);
            var html = this.TenantLeasePanel(model);
            return Json(new { Id = tenantLease.Id, IsTermIsMonthToMonth = tenantLease.TermIsMonthToMonth, IsBiMonthly = tenantLease.DueFrequency, Html = html });
        }

        [NonAction]
        public string TenantLeasePanel(TenantLeaseModel model)
        {
            var html = this.RenderPartialViewToString("_TenantLeaseViewDetails", model);
            return html;
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Portal")]
        public ActionResult CancelTenantLease(long id)
        {
            var tenantLease = _tenantLeaseRepository.GetById(id);
            if (tenantLease.IsNew == true)
            {
                _tenantLeaseRepository.DeleteAndCommit(tenantLease);
            }
            return new NullJsonResult();
        }

        #endregion

        #region TenantPayments

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Portal")]
        public ActionResult RentPaymentList(long tenantId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _tenantPaymentRepository.GetAll().Where(c => c.TenantId == tenantId && c.ChargeType.Name == "Rent Fee");
            query = sort == null ? query.OrderBy(a => a.Name) : query.Sort(sort);
            var data = new PagedList<TenantPayment>(query, command.Page - 1, command.PageSize);
            var result = data.Select(x => x.ToModel()).ToList();
            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = data.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Property.Tenant.Portal")]
        public ActionResult ChargePaymentList(long tenantId, int? chargeDueType, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _tenantPaymentRepository.GetAll().Where(c => c.TenantId == tenantId);

            if (chargeDueType == (int)(ChargeDueType.OnceOnlyWhenTheLeaseStarts) || chargeDueType == (int)(ChargeDueType.OnceOnlyWhenTheLeaseEnds))
            {
                query = query.Where(c => c.TenantLeaseCharge.ChargeDueType == (int?)ChargeDueType.OnceOnlyWhenTheLeaseStarts || c.TenantLeaseCharge.ChargeDueType == (int?)ChargeDueType.OnceOnlyWhenTheLeaseEnds);
            }
            else
            {
                query = query.Where(c => c.TenantLeaseCharge.ChargeDueType == chargeDueType);
            }
            query = sort == null ? query.OrderBy(a => a.Name) : query.Sort(sort);
            var data = new PagedList<TenantPayment>(query, command.Page - 1, command.PageSize);
            var result = data.Select(x => x.ToModel()).ToList();
            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = data.TotalCount
            };

            return Json(gridModel);
        }

        #endregion
    }
}