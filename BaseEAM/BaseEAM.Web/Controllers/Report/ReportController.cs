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
using BaseEAM.Web.Framework.Filters;
using BaseEAM.Web.Framework;
using System;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;

namespace BaseEAM.Web.Controllers
{
    public class ReportController : BaseController
    {
        #region Fields

        private readonly IRepository<Report> _reportRepository;
        private readonly IRepository<ReportFilter> _reportFilterRepository;
        private readonly IRepository<ReportColumn> _reportColumnRepository;
        private readonly IRepository<SecurityGroup> _securityGroupRepository;
        private readonly IRepository<Core.Domain.Filter> _filterRepository;
        private readonly IReportService _reportService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly HttpContextBase _httpContext;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Constructors

        public ReportController(IRepository<Report> reportRepository,
            IRepository<ReportFilter> reportFilterRepository,
            IRepository<ReportColumn> reportColumnRepository,
            IRepository<SecurityGroup> securityGroupRepository,
            IRepository<Core.Domain.Filter> filterRepository,
            IReportService reportService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            HttpContextBase httpContext,
            IWorkContext workContext,
            IDbContext dbContext)
        {
            this._reportRepository = reportRepository;
            this._reportFilterRepository = reportFilterRepository;
            this._reportColumnRepository = reportColumnRepository;
            this._securityGroupRepository = securityGroupRepository;
            this._filterRepository = filterRepository;
            this._localizationService = localizationService;
            this._reportService = reportService;
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
            var reportNameFilter = new FieldModel
            {
                DisplayOrder = 1,
                Name = "ReportName",
                ResourceKey = "Report.Name",
                DbColumn = "Name, Description, Type",
                Value = null,
                ControlType = FieldControlType.TextBox,
                DataType = FieldDataType.String,
                DataSource = FieldDataSource.None,
                IsRequiredField = false
            };
            model.Filters.Add(reportNameFilter);

            return model;
        }

        #endregion

        #region Reports

        [BaseEamAuthorize(PermissionNames = "Report.Report.Read")]
        public ActionResult List()
        {
            var model = _httpContext.Session[SessionKey.ReportSearchModel] as SearchModel;
            //If not exist, build search model
            if (model == null)
            {
                model = BuildSearchModel();
                //session save
                _httpContext.Session[SessionKey.ReportSearchModel] = model;
            }
            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Read")]
        public ActionResult List(DataSourceRequest command, string searchValues, IEnumerable<Sort> sort = null)
        {
            var model = _httpContext.Session[SessionKey.ReportSearchModel] as SearchModel;
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
                _httpContext.Session[SessionKey.ReportSearchModel] = model;

                PagedResult<Report> data = _reportService.GetReports(model.ToExpression(), model.ToParameters(), command.Page - 1, command.PageSize, sort);

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
        [BaseEamAuthorize(PermissionNames = "Report.Report.Create")]
        public ActionResult Create()
        {
            var report = new Report { IsNew = true, IncludeCurrentUserInQuery = true };
            _reportRepository.InsertAndCommit(report);
            return Json(new { Id = report.Id });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Create")]
        public ActionResult Cancel(long? parentId, long id)
        {
            this._dbContext.DeleteById<Report>(id);
            return new NullJsonResult();
        }

        [BaseEamAuthorize(PermissionNames = "Report.Report.Create,Report.Report.Read,Report.Report.Update")]
        public ActionResult Edit(long id)
        {
            var report = _reportRepository.GetById(id);
            var model = report.ToModel();

            return View(model);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Create,Report.Report.Update")]
        public ActionResult Edit(ReportModel model)
        {
            var report = _reportRepository.GetById(model.Id);
            if (ModelState.IsValid)
            {
                report = model.ToEntity(report);
                //always set IsNew to false when saving
                report.IsNew = false;
                _reportRepository.Update(report);

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
        [BaseEamAuthorize(PermissionNames = "Report.Report.Delete")]
        public ActionResult Delete(long? parentId, long id)
        {
            var report = _reportRepository.GetById(id);

            if (!_reportService.IsDeactivable(report))
            {
                ModelState.AddModelError("Report", _localizationService.GetResource("Common.NotDeactivable"));
            }

            if (ModelState.IsValid)
            {
                //soft delete
                _reportRepository.DeactivateAndCommit(report);
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
        [BaseEamAuthorize(PermissionNames = "Report.Report.Delete")]
        public ActionResult DeleteSelected(long? parentId, ICollection<long> selectedIds)
        {
            var reports = new List<Report>();
            foreach (long id in selectedIds)
            {
                var report = _reportRepository.GetById(id);
                if (report != null)
                {
                    if (!_reportService.IsDeactivable(report))
                    {
                        ModelState.AddModelError("Report", _localizationService.GetResource("Common.NotDeactivable"));
                        break;
                    }
                    else
                    {
                        reports.Add(report);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                foreach (var report in reports)
                    _reportRepository.Deactivate(report);
                this._dbContext.SaveChanges();
                SuccessNotification(_localizationService.GetResource("Record.Deleted"));
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.SerializeErrors() });
            }
        }

        [HttpPost]
        public ActionResult ReportInfo(long? reportId)
        {
            if (reportId == null || reportId == 0)
                return new NullJsonResult();

            var reportInfo = _reportRepository.GetById(reportId).ToModel();
            return Json(new { reportInfo = reportInfo });
        }

        public FileResult ExportReport(long reportId)
        {
            var report = _reportRepository.GetById(reportId);
            // clone report to serialize
            dynamic exportReport = new JObject();
            exportReport.Id = report.Id;
            exportReport.Name = report.Name;
            exportReport.Version = report.Version;
            exportReport.IsNew = report.IsNew;
            exportReport.IsDeleted = report.IsDeleted;
            exportReport.CreatedUser = report.CreatedUser;
            exportReport.CreatedDateTime = report.CreatedDateTime;
            exportReport.ModifiedUser = report.ModifiedUser;
            exportReport.ModifiedDateTime = report.ModifiedDateTime;
            exportReport.Description = report.Description;
            exportReport.Type = report.Type;
            exportReport.TemplateType = report.TemplateType;
            exportReport.TemplateFileName = report.TemplateFileName;
            exportReport.TemplateFileBytes = report.TemplateFileBytes;
            exportReport.Query = report.Query;
            exportReport.SortExpression = report.SortExpression;
            exportReport.IncludeCurrentUserInQuery = report.IncludeCurrentUserInQuery;
            exportReport.ReportFilters = new JArray();
            exportReport.ReportColumns = new JArray();
            exportReport.SecurityGroups = new JArray();
            foreach (var reportFilter in report.ReportFilters)
            {
                dynamic exportReportFilter = new JObject();
                exportReportFilter.Id = reportFilter.Id;
                exportReportFilter.Name = reportFilter.Name;
                exportReportFilter.Version = reportFilter.Version;
                exportReportFilter.IsNew = reportFilter.IsNew;
                exportReportFilter.IsDeleted = reportFilter.IsDeleted;
                exportReportFilter.CreatedUser = reportFilter.CreatedUser;
                exportReportFilter.CreatedDateTime = reportFilter.CreatedDateTime;
                exportReportFilter.ModifiedUser = reportFilter.ModifiedUser;
                exportReportFilter.ModifiedDateTime = reportFilter.ModifiedDateTime;
                exportReportFilter.ReportId = reportFilter.ReportId;
                exportReportFilter.FilterId = reportFilter.FilterId;
                exportReportFilter.DisplayOrder = reportFilter.DisplayOrder;
                exportReportFilter.DbColumn = reportFilter.DbColumn;
                exportReportFilter.IsRequired = reportFilter.IsRequired;
                exportReportFilter.ResourceKey = reportFilter.ResourceKey;
                exportReportFilter.ParentReportFilterId = reportFilter.ParentReportFilterId;

                exportReport.ReportFilters.Add(exportReportFilter);
            }
            foreach(var reportColumn in report.ReportColumns)
            {
                dynamic exportReportColumn = new JObject();
                exportReportColumn.Id = reportColumn.Id;
                exportReportColumn.Name = reportColumn.Name;
                exportReportColumn.Version = reportColumn.Version;
                exportReportColumn.IsNew = reportColumn.IsNew;
                exportReportColumn.IsDeleted = reportColumn.IsDeleted;
                exportReportColumn.CreatedUser = reportColumn.CreatedUser;
                exportReportColumn.CreatedDateTime = reportColumn.CreatedDateTime;
                exportReportColumn.ModifiedUser = reportColumn.ModifiedUser;
                exportReportColumn.ModifiedDateTime = reportColumn.ModifiedDateTime;
                exportReportColumn.ReportId = reportColumn.ReportId;
                exportReportColumn.DisplayOrder = reportColumn.DisplayOrder;
                exportReportColumn.ColumnName = reportColumn.ColumnName;
                exportReportColumn.DataType = reportColumn.DataType;
                exportReportColumn.FormatString = reportColumn.FormatString;
                exportReportColumn.ResourceKey = reportColumn.ResourceKey;

                exportReport.ReportColumns.Add(exportReportColumn);
            }
            foreach(var securityGroup in report.SecurityGroups)
            {
                dynamic exportSecurityGroup = new JObject();
                exportSecurityGroup.Id = securityGroup.Id;

                exportReport.SecurityGroups.Add(exportSecurityGroup);
            }
            string json = JsonConvert.SerializeObject(exportReport);
            return File(Encoding.ASCII.GetBytes(json), "application/json", report.Name + ".json");
        }

        [HttpPost]
        public ActionResult ImportReport()
        {
            try
            {
                var file = Request.Files["importfile"];
                if (file != null && file.ContentLength > 0)
                {
                    using (var sr = new StreamReader(file.InputStream, Encoding.UTF8))
                    {
                        string content = sr.ReadToEnd();
                        var importReport = JsonConvert.DeserializeObject<Report>(content);
                        var existingReport = _reportRepository.GetAll()
                            .Where(r => r.Name == importReport.Name && r.Type == importReport.Type)
                            .FirstOrDefault();
                        if (existingReport == null)
                        {
                            InsertReport(importReport);
                        }
                        else
                        {
                            DeleteReport(existingReport);
                            InsertReport(importReport);
                        }
                    }
                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Common.UploadFile"));
                    return RedirectToAction("List");
                }

                SuccessNotification(_localizationService.GetResource("Report.Imported"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        private void InsertReport(Report importReport)
        {
            var report = new Report();
            report.Name = importReport.Name;
            report.Version = importReport.Version;
            report.IsNew = importReport.IsNew;
            report.IsDeleted = importReport.IsDeleted;
            report.CreatedUser = importReport.CreatedUser;
            report.CreatedDateTime = importReport.CreatedDateTime;
            report.ModifiedUser = importReport.ModifiedUser;
            report.ModifiedDateTime = importReport.ModifiedDateTime;
            report.Description = importReport.Description;
            report.Type = importReport.Type;
            report.TemplateType = importReport.TemplateType;
            report.TemplateFileName = importReport.TemplateFileName;
            report.TemplateFileBytes = importReport.TemplateFileBytes;
            report.Query = importReport.Query;
            report.SortExpression = importReport.SortExpression;
            report.IncludeCurrentUserInQuery = importReport.IncludeCurrentUserInQuery;

            _reportRepository.Insert(report);

            foreach(var importReportFilter in importReport.ReportFilters)
            {
                var reportFilter = new ReportFilter();
                reportFilter.Name = importReportFilter.Name;
                reportFilter.Version = importReportFilter.Version;
                reportFilter.IsNew = importReportFilter.IsNew;
                reportFilter.IsDeleted = importReportFilter.IsDeleted;
                reportFilter.CreatedUser = importReportFilter.CreatedUser;
                reportFilter.CreatedDateTime = importReportFilter.CreatedDateTime;
                reportFilter.ModifiedUser = importReportFilter.ModifiedUser;
                reportFilter.ModifiedDateTime = importReportFilter.ModifiedDateTime;
                reportFilter.ReportId = importReportFilter.ReportId;
                reportFilter.FilterId = importReportFilter.FilterId;
                reportFilter.DisplayOrder = importReportFilter.DisplayOrder;
                reportFilter.DbColumn = importReportFilter.DbColumn;
                reportFilter.IsRequired = importReportFilter.IsRequired;
                reportFilter.ResourceKey = importReportFilter.ResourceKey;
                reportFilter.ParentReportFilterId = importReportFilter.ParentReportFilterId;

                _reportFilterRepository.Insert(reportFilter);
                report.ReportFilters.Add(reportFilter);
            }

            foreach (var importReportColumn in importReport.ReportColumns)
            {
                var reportColumn = new ReportColumn();
                reportColumn.Name = importReportColumn.Name;
                reportColumn.Version = importReportColumn.Version;
                reportColumn.IsNew = importReportColumn.IsNew;
                reportColumn.IsDeleted = importReportColumn.IsDeleted;
                reportColumn.CreatedUser = importReportColumn.CreatedUser;
                reportColumn.CreatedDateTime = importReportColumn.CreatedDateTime;
                reportColumn.ModifiedUser = importReportColumn.ModifiedUser;
                reportColumn.ModifiedDateTime = importReportColumn.ModifiedDateTime;
                reportColumn.ReportId = importReportColumn.ReportId;
                reportColumn.DisplayOrder = importReportColumn.DisplayOrder;
                reportColumn.ColumnName = importReportColumn.ColumnName;
                reportColumn.DataType = importReportColumn.DataType;
                reportColumn.FormatString = importReportColumn.FormatString;
                reportColumn.ResourceKey = importReportColumn.ResourceKey;

                _reportColumnRepository.Insert(reportColumn);
                report.ReportColumns.Add(reportColumn);
            }

            foreach(var importSecurityGroup in importReport.SecurityGroups)
            {
                var securityGroup = _securityGroupRepository.GetById(importSecurityGroup.Id);
                report.SecurityGroups.Add(securityGroup);
            }

            this._dbContext.SaveChanges();
        }

        private void DeleteReport(Report existingReport)
        {
            _reportRepository.DeleteAndCommit(existingReport);
        }

        #endregion

        #region ReportFilter
        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Read,Report.ReportFilter.Read")]
        public ActionResult ReportFilterList(long reportId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _reportFilterRepository.GetAll().Where(c => c.ReportId == reportId);
            query = sort == null ? query.OrderBy(a => a.DisplayOrder) : query.Sort(sort);
            var reportFilters = new PagedList<ReportFilter>(query, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = reportFilters.Select(x => x.ToModel()),
                Total = reportFilters.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Read,Report.Report.Create,Report.Report.Update")]
        public ActionResult ReportFilter(long id)
        {
            var reportFilter = _reportFilterRepository.GetById(id);
            var model = reportFilter.ToModel();
            var html = this.ReportFilterPanel(model);
            return Json(new { Id = reportFilter.Id, Html = html });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Create")]
        public ActionResult CreateReportFilter(long reportId)
        {
            var report = _reportRepository.GetById(reportId);
            var maxDisplayOrder = report.ReportFilters.Max(p => p.DisplayOrder) ?? 0;
            var reportFilter = new ReportFilter
            {
                ReportId = reportId,
                IsNew = true,
                DisplayOrder = maxDisplayOrder + 1
            };
            _reportFilterRepository.InsertAndCommit(reportFilter);

            var model = new ReportFilterModel();
            model = reportFilter.ToModel();
            var html = this.ReportFilterPanel(model);

            return Json(new { Id = reportFilter.Id, Html = html });
        }

        [NonAction]
        public string ReportFilterPanel(ReportFilterModel model)
        {
            var html = this.RenderPartialViewToString("_ReportFilterDetails", model);
            return html;
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Create,Report.Report.Update")]
        public ActionResult SaveReportFilter(ReportFilterModel model)
        {
            if (ModelState.IsValid)
            {
                var reportFilter = _reportFilterRepository.GetById(model.Id);
                if (string.IsNullOrEmpty(reportFilter.Name))
                {
                    var filter = _filterRepository.GetById(model.FilterId);
                    model.Name = filter.Name + "_" + Guid.NewGuid();
                }
                //always set IsNew to false when saving
                reportFilter.IsNew = false;
                reportFilter = model.ToEntity(reportFilter);

                _reportFilterRepository.UpdateAndCommit(reportFilter);
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.Errors().ToHtmlString() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Create,Report.Report.Update")]
        public ActionResult CancelReportFilter(long id)
        {
            var reportFilter = _reportFilterRepository.GetById(id);
            if (reportFilter.IsNew == true)
            {
                _reportFilterRepository.DeleteAndCommit(reportFilter);
            }
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Delete")]
        public ActionResult DeleteReportFilter(long? parentId, long id)
        {
            var reportFilter = _reportFilterRepository.GetById(id);
            //For parent-child, we can mark deleted to child
            _reportFilterRepository.DeactivateAndCommit(reportFilter);
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Delete")]
        public ActionResult DeleteSelectedReportFilters(long? parentId, long[] selectedIds)
        {
            foreach (long id in selectedIds)
            {
                var reportFilter = _reportFilterRepository.GetById(id);
                //For parent-child, we can mark deleted to child
                _reportFilterRepository.Deactivate(reportFilter);
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        /// <summary>
        /// Get the filters of the current report 
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ParentReportFilterList(long reportId, string param)
        {
            var filters = _reportFilterRepository.GetAll()
                .Where(f => f.ReportId == reportId && f.Filter.Name.Contains(param))
                .Select(f => new SelectListItem { Text = f.Filter.Name, Value = f.Id.ToString() })
                .ToList();
            if (filters.Count > 0)
            {
                filters.Insert(0, new SelectListItem { Value = "", Text = "" });
            }
            return Json(filters);
        }

        #endregion

        #region ReportColumn

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Read,Report.ReportColumn.Read")]
        public ActionResult ReportColumnList(long reportId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var query = _reportColumnRepository.GetAll().Where(c => c.ReportId == reportId);
            query = sort == null ? query.OrderBy(a => a.DisplayOrder) : query.Sort(sort);
            var reportColumns = new PagedList<ReportColumn>(query, command.Page - 1, command.PageSize);
            var result = reportColumns.Select(x => x.ToModel()).ToList();
            var gridModel = new DataSourceResult
            {
                Data = result,
                Total = reportColumns.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Read,Report.ReportColumn.Create,Report.ReportColumn.Read,Report.ReportColumn.Update")]
        public ActionResult ReportColumn(long id)
        {
            var reportColumn = _reportColumnRepository.GetById(id);
            var model = reportColumn.ToModel();
            var html = this.ReportColumnPanel(model);
            return Json(new { Id = reportColumn.Id, Html = html });
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Create")]
        public ActionResult CreateReportColumn(long reportId)
        {
            var report = _reportRepository.GetById(reportId);
            var maxDisplayOrder = report.ReportColumns.Max(p => p.DisplayOrder) ?? 0;
            var reportColumn = new ReportColumn
            {
                ReportId = reportId,
                IsNew = true,
                DisplayOrder = maxDisplayOrder + 1
            };
            _reportColumnRepository.InsertAndCommit(reportColumn);

            var model = new ReportColumnModel();
            model = reportColumn.ToModel();
            var html = this.ReportColumnPanel(model);

            return Json(new { Id = reportColumn.Id, Html = html });
        }

        [NonAction]
        public string ReportColumnPanel(ReportColumnModel model)
        {
            var html = this.RenderPartialViewToString("_ReportColumnDetails", model);
            return html;
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Create,Report.Report.Update")]
        public ActionResult SaveReportColumn(ReportColumnModel model)
        {
            if (ModelState.IsValid)
            {
                var reportColumn = _reportColumnRepository.GetById(model.Id);
                //always set IsNew to false when saving
                reportColumn.IsNew = false;
                reportColumn = model.ToEntity(reportColumn);

                _reportColumnRepository.UpdateAndCommit(reportColumn);
                return new NullJsonResult();
            }
            else
            {
                return Json(new { Errors = ModelState.Errors().ToHtmlString() });
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Create,Report.Report.Update")]
        public ActionResult CancelReportColumn(long id)
        {
            var reportColumn = _reportColumnRepository.GetById(id);
            if (reportColumn.IsNew == true)
            {
                _reportColumnRepository.DeleteAndCommit(reportColumn);
            }
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Delete")]
        public ActionResult DeleteReportColumn(long? parentId, long id)
        {
            var reportColumn = _reportColumnRepository.GetById(id);
            //For parent-child, we can mark deleted to child
            _reportColumnRepository.DeactivateAndCommit(reportColumn);
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Delete")]
        public ActionResult DeleteSelectedReportColumns(long? parentId, long[] selectedIds)
        {
            foreach (long id in selectedIds)
            {
                var reportColumn = _reportColumnRepository.GetById(id);
                //For parent-child, we can mark deleted to child
                _reportColumnRepository.Deactivate(reportColumn);
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        #endregion

        #region SecurityGroups

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Read")]
        public ActionResult SecurityGroupList(long reportId, DataSourceRequest command, IEnumerable<Sort> sort = null)
        {
            var sites = _reportRepository.GetById(reportId).SecurityGroups;
            if (sites.Count == 0)
            {
                return Json(new DataSourceResult());
            }
            else
            {
                var queryable = sites.AsQueryable<SecurityGroup>();
                queryable = sort == null ? queryable.OrderBy(a => a.CreatedDateTime) : queryable.Sort(sort);
                var data = queryable.ToList().Select(x => x.ToModel()).ToList();
                var gridModel = new DataSourceResult
                {
                    Data = data.PagedForCommand(command),
                    Total = sites.Count()
                };

                return Json(gridModel);
            }
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Create,Report.Report.Update")]
        public ActionResult AddSecurityGroups(long reportId, long[] selectedIds)
        {
            var report = _reportRepository.GetById(reportId);
            foreach (var id in selectedIds)
            {
                var existed = report.SecurityGroups.Any(s => s.Id == id);
                if (!existed)
                {
                    var securityGroup = _securityGroupRepository.GetById(id);
                    report.SecurityGroups.Add(securityGroup);
                }
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Create,Report.Report.Update")]
        public ActionResult DeleteSecurityGroup(long? parentId, long id)
        {
            var report = _reportRepository.GetById(parentId);
            var securityGroup = _securityGroupRepository.GetById(id);
            //For many-many, delete by set foreign key to null
            report.SecurityGroups.Remove(securityGroup);

            _reportRepository.UpdateAndCommit(report);
            return new NullJsonResult();
        }

        [HttpPost]
        [BaseEamAuthorize(PermissionNames = "Report.Report.Create,Report.Report.Update")]
        public ActionResult DeleteSelectedSecurityGroups(long? parentId, long[] selectedIds)
        {
            var report = _reportRepository.GetById(parentId);
            foreach (long id in selectedIds)
            {
                var securityGroup = _securityGroupRepository.GetById(id);
                //For many-many, need to remove from parent
                report.SecurityGroups.Remove(securityGroup);
            }
            this._dbContext.SaveChanges();
            return new NullJsonResult();
        }

        #endregion

    }
}