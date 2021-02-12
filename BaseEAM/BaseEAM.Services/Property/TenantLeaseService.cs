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
using System;

namespace BaseEAM.Services
{
    public class TenantLeaseService : BaseService, ITenantLeaseService
    {
        #region Fields

        private readonly IRepository<TenantLease> _tenantLeaseRepository;
        private readonly IRepository<TenantPayment> _tenantPaymentRepository;
        private readonly IRepository<TenantLeasePaymentSchedule> _tenantLeasePaymentScheduleRepository;
        private readonly IRepository<TenantLeaseCharge> _tenantLeaseChargeRepository;
        private readonly IRepository<ValueItem> _valueItemRepository;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IRepository<User> _userRepository;
        private readonly DapperContext _dapperContext;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public TenantLeaseService(IRepository<TenantLease> tenantLeaseRepository,
            IRepository<TenantPayment> tenantPaymentRepository,
            IRepository<TenantLeasePaymentSchedule> tenantLeasePaymentScheduleRepository,
            IRepository<TenantLeaseCharge> tenantLeaseChargeRepository,
            IRepository<ValueItem> valueItemRepository,
            IDateTimeHelper dateTimeHelper,
            IRepository<User> userRepository,
            DapperContext dapperContext,
            IWorkContext workContext,
            IDbContext dbContext)
        {
            this._tenantLeaseRepository = tenantLeaseRepository;
            this._tenantPaymentRepository = tenantPaymentRepository;
            this._tenantLeasePaymentScheduleRepository = tenantLeasePaymentScheduleRepository;
            this._tenantLeaseChargeRepository = tenantLeaseChargeRepository;
            this._valueItemRepository = valueItemRepository;
            this._dateTimeHelper = dateTimeHelper;
            this._userRepository = userRepository;
            this._dapperContext = dapperContext;
            this._workContext = workContext;
            this._dbContext = dbContext;
        }

        #endregion

        #region Methods

        public virtual PagedResult<TenantLease> GetTenantLeases(string expression,
            dynamic parameters,
            int pageIndex = 0,
            int pageSize = 2147483647,
            IEnumerable<Sort> sort = null)
        {
            var searchBuilder = new SqlBuilder();
            var search = searchBuilder.AddTemplate(SqlTemplate.TenantLeaseSearch(), new { skip = pageIndex * pageSize, take = pageSize });
            if (!string.IsNullOrEmpty(expression))
                searchBuilder.Where(expression, parameters);
            if (sort != null)
            {
                foreach (var s in sort)
                    searchBuilder.OrderBy(s.ToExpression());
            }
            else
            {
                searchBuilder.OrderBy("TenantLease.Number");
            }

            var countBuilder = new SqlBuilder();
            var count = countBuilder.AddTemplate(SqlTemplate.TenantLeaseSearchCount());
            if (!string.IsNullOrEmpty(expression))
                countBuilder.Where(expression, parameters);

            using (var connection = _dapperContext.GetOpenConnection())
            {
                var tenantLeases = connection.Query<TenantLease, Site, Tenant, Property, Assignment, TenantLease>(search.RawSql,
                    (tenantLease, site, tenant, property, assignment) => { tenantLease.Site = site; tenantLease.Tenant = tenant; tenantLease.Property = property; tenantLease.Assignment = assignment; return tenantLease; }, search.Parameters);
                var totalCount = connection.Query<int>(count.RawSql, search.Parameters).Single();
                return new PagedResult<TenantLease>(tenantLeases, totalCount);
            }
        }

        public virtual List<User> GetCreatedUser(long id)
        {
            var result = new List<User>();
            var tenantLease = _tenantLeaseRepository.GetById(id);
            var createdUser = _userRepository.GetById(tenantLease.CreatedUserId);
            result.Add(createdUser);
            return result;
        }

        public virtual void CreatePaymentSchedule(TenantLease tenantLease, DateTime? startDate)
        {
            var tenantLeasePaymentSchedule = new TenantLeasePaymentSchedule
            {
                TenantLeaseId = tenantLease.Id,
                DueAmount = tenantLease.TermRentAmount,
                DueDate = startDate,
                IsNew = false,
            };
            _tenantLeasePaymentScheduleRepository.Insert(tenantLeasePaymentSchedule);
        }


        /// <summary>
        /// Find the next day based on numberMonth and biMonthly(if yes)
        /// </summary>
        /// <param name="tenantLease"></param>
        /// <param name="numberMonth"></param>
        /// <param name="biMonthly"></param>
        /// <returns></returns>
        private DateTime GenerateNextDate(TenantLease tenantLease, int numberMonth, int biMonthly = 0)
        {
            var tempTermStartDateUserTime = _dateTimeHelper.ConvertToUserTime(tenantLease.FirstPaymentDate.Value, DateTimeKind.Utc);
            int dayOfMonth = tempTermStartDateUserTime.Day;
            if (tenantLease.DueFrequency == (int)DueFrequency.BiMonthly)
            {
                dayOfMonth = biMonthly;
            }
            var nextDate = new DateTime(tempTermStartDateUserTime.Year, tempTermStartDateUserTime.Month, 1).AddMonths(numberMonth);
            nextDate = GenerateNextDay(nextDate, dayOfMonth);
            return nextDate;
        }

        private DateTime GenerateNextDay(DateTime nextDate, int dayOfMonth)
        {
            var daysOfNextMonth = DateTime.DaysInMonth(nextDate.Year, nextDate.Month);
            while (true)
            {
                if (daysOfNextMonth >= dayOfMonth)
                {
                    return new DateTime(nextDate.Year, nextDate.Month, dayOfMonth);
                }
                else
                {
                    dayOfMonth = dayOfMonth - 1;
                }
            }

        }

        /// <summary>
        // nextDate must less than or equal termEndDate.
        /// </summary>
        /// <param name="nextDate"></param>
        /// <param name="tenantLease"></param>
        /// <returns></returns>
        private bool IsValidDate(DateTime? nextDate, DateTime? termEndDate)
        {
            if (nextDate.HasValue && nextDate <= termEndDate)
            {
                return true;
            }
            return false;
        }

        private void DeletePaymentSchedules(TenantLease tenantLease)
        {
            var tenantLeasePaymentSchedules = _tenantLeasePaymentScheduleRepository.GetAll().Where(t => t.TenantLeaseId == tenantLease.Id).ToList();
            foreach (var item in tenantLeasePaymentSchedules)
            {
                _tenantLeasePaymentScheduleRepository.Delete(item);
            }
        }

        public DateTime? GetDueDate(TenantLease tenantLease, TenantLeaseCharge tenantLeaseCharge)
        {
            var chargeDueType = (ChargeDueType)tenantLeaseCharge.ChargeDueType;
            switch (chargeDueType)
            {
                case ChargeDueType.OnceOnlyWhenTheLeaseStarts:
                    return tenantLease.TermStartDate;
                case ChargeDueType.OnceOnlyWhenTheLeaseEnds:
                    return tenantLease.TermEndDate;
                case ChargeDueType.OnceOnlyOnaSpecificDate:
                    return tenantLeaseCharge.ChargeDueDate;
                case ChargeDueType.MonthlyOnaSpecificDay:
                    var chargeDueDate = GenerateNextDay(tenantLeaseCharge.ValidFrom.Value, tenantLeaseCharge.ChargeDueDay.Value);
                    if (chargeDueDate >= tenantLeaseCharge.ValidFrom.Value && chargeDueDate <= tenantLeaseCharge.ValidTo.Value)
                    {
                        return chargeDueDate;
                    }
                    return null;
                case ChargeDueType.EachTimeRentIsDue:
                default:
                    return null;
            };
        }

        public virtual void CreateRentPayment(TenantLeasePaymentSchedule paymentSchedule, DateTime? startDate)
        {
            var tenantLease = paymentSchedule.TenantLease;
            var chargeType = _valueItemRepository.GetAll().Where(v => v.Name == "Rent Fee").FirstOrDefault();
            var tenantPayment = new TenantPayment
            {
                SiteId = tenantLease.SiteId,
                TenantId = tenantLease.TenantId,
                PropertyId = tenantLease.PropertyId,
                TenantLeaseId = tenantLease.Id,
                TenantLeasePaymentScheduleId = paymentSchedule.Id,
                DueDate = startDate,
                DueAmount = paymentSchedule.DueAmount,
                ChargeTypeId = chargeType.Id,
                CollectedAmount = 0,
                BalanceAmount = paymentSchedule.DueAmount
            };
            _tenantPaymentRepository.Insert(tenantPayment);
        }

        public virtual void GeneratePaymentSchedules(TenantLease tenantLease)
        {
            if (tenantLease.TermEndDate == null)
            {
                tenantLease.TermEndDate = tenantLease.TermStartDate.Value.AddYears(20);
            }
            var tenantPayments = _tenantPaymentRepository.GetAll().Where(p => p.TenantLeaseId == tenantLease.Id && p.TenantLeasePaymentScheduleId != null).ToList();
            foreach (var tenantPayment in tenantPayments)
            {
                _tenantPaymentRepository.Delete(tenantPayment);
            }
            
            DeletePaymentSchedules(tenantLease);

            DueFrequency dueFrequency = (DueFrequency)tenantLease.DueFrequency;
            DateTime termStartDate = tenantLease.TermStartDate.Value;
            DateTime termEndDate = tenantLease.TermEndDate.Value;
            DateTime firstPaymentDate = tenantLease.FirstPaymentDate.Value;

            //Create first payment schedule
            CreatePaymentSchedule(tenantLease, firstPaymentDate);
            int numberOfDays = (int)(termEndDate - firstPaymentDate.Date).TotalDays;
            int numberOfWeeks = numberOfDays / 7;
            int numberOfMonths = ((termEndDate.Year - termStartDate.Year) * 12) + termEndDate.Month - termStartDate.Month;
            switch (dueFrequency)
            {
                case DueFrequency.Monthly:
                    for (int i = 0; i < numberOfMonths; i++)
                    {
                        termStartDate = GenerateNextDate(tenantLease, i + 1);
                        if (IsValidDate(termStartDate, termEndDate) == false)
                        {
                            break;
                        }
                        CreatePaymentSchedule(tenantLease, termStartDate);
                    }
                    break;
                case DueFrequency.EveryTwoWeeks:
                    numberOfWeeks = numberOfDays / 7;
                    for (int i = 0; i < numberOfWeeks; i++)
                    {
                        termStartDate = firstPaymentDate.AddDays(7 * (i + 1) * 2);
                        if (IsValidDate(termStartDate, termEndDate) == false)
                        {
                            break;
                        }

                        CreatePaymentSchedule(tenantLease, termStartDate);
                    }
                    break;
                case DueFrequency.Weekly:
                    numberOfWeeks = numberOfDays / 7;
                    for (int i = 0; i < numberOfWeeks; i++)
                    {
                        termStartDate = firstPaymentDate.AddDays(7 * (i + 1));
                        if (IsValidDate(termStartDate, termEndDate) == false)
                        {
                            break;
                        }
                        CreatePaymentSchedule(tenantLease, termStartDate);
                    }
                    break;
                case DueFrequency.Daily:
                    for (int i = 0; i < numberOfDays; i++)
                    {
                        termStartDate = firstPaymentDate.AddDays(i + 1);
                        if (IsValidDate(termStartDate, termEndDate) == false)
                        {
                            break;
                        }
                        CreatePaymentSchedule(tenantLease, termStartDate);
                    }
                    break;
                case DueFrequency.Quarterly:
                    for (int i = 0; i < numberOfMonths; i++)
                    {
                        termStartDate = GenerateNextDate(tenantLease, 3 * (i + 1));
                        if (IsValidDate(termStartDate, termEndDate) == false)
                        {
                            break;
                        }
                        CreatePaymentSchedule(tenantLease, termStartDate);
                    }
                    break;
                case DueFrequency.Every6Months:
                    for (int i = 0; i < numberOfMonths; i++)
                    {
                        termStartDate = GenerateNextDate(tenantLease, 3 * 2 * (i + 1));
                        if (IsValidDate(termStartDate, termEndDate) == false)
                        {
                            break;
                        }
                        CreatePaymentSchedule(tenantLease, termStartDate);
                    }
                    break;
                case DueFrequency.Yearly:
                    int numberOfYears = termEndDate.Year - termStartDate.Year;
                    for (int i = 0; i < numberOfYears; i++)
                    {
                        termStartDate = GenerateNextDate(tenantLease, 12 * (i + 1));
                        if (IsValidDate(termStartDate, termEndDate) == false)
                        {
                            break;
                        }
                        CreatePaymentSchedule(tenantLease, termStartDate);
                    }
                    break;
                case DueFrequency.BiMonthly:
                    var biMonthlyStart = tenantLease.BiMonthlyStart.Value;
                    var biMonthlyEnd = tenantLease.BiMonthlyEnd.Value;
                    var maxMonthlyStart = biMonthlyStart > biMonthlyEnd ? biMonthlyStart : biMonthlyEnd;
                    //Generate the second payment schedule for first month.
                    var maxMonthlyDate = GenerateNextDate(tenantLease, 0, maxMonthlyStart);
                    if (IsValidDate(maxMonthlyDate, termEndDate) == false)
                    {
                        break;
                    }

                    CreatePaymentSchedule(tenantLease, maxMonthlyDate);

                    for (int i = 0; i < numberOfMonths; i++)
                    {
                        termStartDate = GenerateNextDate(tenantLease, i + 1, biMonthlyStart);
                        if (IsValidDate(termStartDate, termEndDate))
                        {
                            CreatePaymentSchedule(tenantLease, termStartDate);
                        }

                        termStartDate = GenerateNextDate(tenantLease, i + 1, biMonthlyEnd);
                        if (IsValidDate(termStartDate, termEndDate))
                        {
                            CreatePaymentSchedule(tenantLease, termStartDate);
                        }
                    }
                    break;
            }
            this._dbContext.SaveChanges();
        }

        public virtual void CreateChargePayment(TenantLeaseCharge tenantLeaseCharge, DateTime? dueDate)
        {
            var tenantLease = tenantLeaseCharge.TenantLease;
            var tenantPayment = new TenantPayment
            {
                SiteId = tenantLease.SiteId,
                TenantId = tenantLease.TenantId,
                PropertyId = tenantLease.PropertyId,
                TenantLeaseId = tenantLease.Id,
                TenantLeaseChargeId = tenantLeaseCharge.Id,
                DueDate = dueDate,
                ChargeTypeId = tenantLeaseCharge.ChargeTypeId,
                DueAmount = tenantLeaseCharge.ChargeAmount,
                CollectedAmount = 0,
                BalanceAmount = tenantLeaseCharge.ChargeAmount
            };

            //Will create payment only when has DueDate
            if (tenantPayment.DueDate.HasValue)
            {
                _tenantPaymentRepository.Insert(tenantPayment);
            }

        }

        public void UpdatePayment(TenantLease tenantLease)
        {
            //Generate a new rent payment
            var tenantPayments = _tenantPaymentRepository.GetAll().Where(p => p.TenantLeaseId == tenantLease.Id).ToList();
            var paymentSchedules = _tenantLeasePaymentScheduleRepository.GetAll()
                .Where(p => p.TenantLeaseId == tenantLease.Id).OrderBy(p => p.DueDate).ToList();
            var countAdditionalPayments = 0;
            var currentDate = DateTime.UtcNow;
            var maxAdditionalPayment = 2;
            foreach (var paymentSchedule in paymentSchedules)
            {
                var existingPayment = tenantPayments.Where(p => p.TenantLeasePaymentScheduleId == paymentSchedule.Id).FirstOrDefault();
                if(paymentSchedule.DueDate >= currentDate)
                {
                    countAdditionalPayments++;
                }
                if(countAdditionalPayments > maxAdditionalPayment)
                {
                    break;
                }
                if (existingPayment == null)
                {
                    CreateRentPayment(paymentSchedule, paymentSchedule.DueDate);
                    paymentSchedules.Add(paymentSchedule);
                    break;
                }

            }
            this._dbContext.SaveChanges();

            countAdditionalPayments = 0;
            //Generate a new charge payment with option: MonthlyOnaSpecificDay
            var tenantLeaseCharges = _tenantLeaseChargeRepository.GetAll().Where(c => c.TenantLeaseId == tenantLease.Id && c.ChargeDueType == (int?)ChargeDueType.MonthlyOnaSpecificDay).ToList();
            foreach (var tenantLeaseCharge in tenantLeaseCharges)
            {
                var validFrom = tenantLeaseCharge.ValidFrom;
                var validTo = tenantLeaseCharge.ValidTo;
                var latestPayment = tenantPayments.Where(p => p.TenantLeaseChargeId == tenantLeaseCharge.Id && p.ChargeTypeId == tenantLeaseCharge.ChargeTypeId).OrderByDescending(p => p.DueDate).FirstOrDefault();
                var latestChargeDueDate = latestPayment != null && latestPayment.DueDate.HasValue ? latestPayment.DueDate.Value : validFrom.Value;

                   var chargeDueDate = GenerateNextDay(latestChargeDueDate, tenantLeaseCharge.ChargeDueDay.Value).AddMonths(1);;
                var existingPayment = tenantPayments.Where(p => p.TenantLeaseChargeId == tenantLeaseCharge.Id && p.DueDate == chargeDueDate).FirstOrDefault();
                int numberOfMonths = ((chargeDueDate.Year - currentDate.Year) * 12) + chargeDueDate.Month - currentDate.Month;

                if (numberOfMonths > 0 && numberOfMonths > maxAdditionalPayment)
                {
                    break;
                }
                if (existingPayment == null && chargeDueDate >= validFrom.Value && chargeDueDate <= validTo.Value)
                {
                    CreateChargePayment(tenantLeaseCharge, chargeDueDate);
                    break;
                }

            }
            this._dbContext.SaveChanges();

            //Generate a new charge payment with option: EachTimeRentIsDue
            tenantLeaseCharges = _tenantLeaseChargeRepository.GetAll().Where(c => c.TenantLeaseId == tenantLease.Id &&c.ChargeDueType == (int?)ChargeDueType.EachTimeRentIsDue).ToList();
            foreach (var tenantLeaseCharge in tenantLeaseCharges)
            {
                var validFrom = tenantLeaseCharge.ValidFrom;
                var validTo = tenantLeaseCharge.ValidTo;

                //filter all payment schedules in range: valid from and valid to
                paymentSchedules = paymentSchedules.Where(c => c.DueDate >= validFrom && c.DueDate <= validTo).ToList();
                foreach (var paymentSchedule in paymentSchedules)
                {
                    var existingPayment = tenantPayments.Where(p => p.TenantLeaseChargeId == tenantLeaseCharge.Id && p.DueDate == paymentSchedule.DueDate).FirstOrDefault();
                    if (paymentSchedule.DueDate >= DateTime.UtcNow)
                    {
                        countAdditionalPayments++;
                    }
                    if (countAdditionalPayments > maxAdditionalPayment)
                    {
                        break;
                    }
                    if (existingPayment == null)
                    {
                        tenantLeaseCharge.ChargeDueDate = paymentSchedule.DueDate;
                        CreateChargePayment(tenantLeaseCharge, paymentSchedule.DueDate);
                        break;
                    }
                }
            }
            this._dbContext.SaveChanges();

            //Late Fee
            if (tenantLease.LateFeeEnabled)
            {
                //get list of rent payment has not collected yet
                tenantPayments = _tenantPaymentRepository.GetAll().Where(p => p.TenantLeaseId == tenantLease.Id && p.TenantLeasePaymentScheduleId != null && p.BalanceAmount > 0).ToList();
                foreach (var tenantPayment in tenantPayments)
                {
                    var numberOfDaysLate = DateTime.UtcNow.Subtract(tenantPayment.DueDate.Value).Days;
                    if (numberOfDaysLate > 0)
                    {
                        if (numberOfDaysLate >= tenantLease.GracePeriodDays.Value)
                        {
                            var lateFeeOption = (LateFeeOption)tenantLease.LateFeeOption;
                            switch (lateFeeOption)
                            {
                                case LateFeeOption.FlatFee:
                                    tenantPayment.LateFeeAmount = tenantLease.FlatFee;
                                    break;
                                case LateFeeOption.BaseAmountPerDay:
                                    tenantPayment.LateFeeAmount = tenantLease.BaseAmountPerDay * tenantPayment.DueAmount + tenantLease.AmountPerDay * numberOfDaysLate;
                                    break;
                                case LateFeeOption.PercentOfRentPerDay:
                                    tenantPayment.LateFeeAmount = (tenantLease.PercentOfRentPerDay * numberOfDaysLate) / 100;
                                    break;
                            }
                            if (tenantPayment.LateFeeAmount <= tenantLease.MaxLateFee)
                            {
                                tenantPayment.BalanceAmount = tenantPayment.DueAmount + tenantPayment.LateFeeAmount - tenantPayment.CollectedAmount;
                            }
                        }
                        tenantPayment.DaysLateCount = numberOfDaysLate;
                        _tenantPaymentRepository.Update(tenantPayment);
                    }
                    this._dbContext.SaveChanges();
                }
            }
        }
        #endregion
    }
}
