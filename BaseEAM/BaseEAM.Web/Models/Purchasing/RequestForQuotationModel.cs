/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Mvc;
using BaseEAM.Web.Validators;
using FluentValidation.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace BaseEAM.Web.Models
{
    [Validator(typeof(RequestForQuotationValidator))]
    public class RequestForQuotationModel : BaseEamEntityModel
    {
        [BaseEamResourceDisplayName("Common.Number")]
        public string Number { get; set; }

        [BaseEamResourceDisplayName("Common.Description")]
        public string Description { get; set; }

        public AssignmentPriority Priority { get; set; }
        [BaseEamResourceDisplayName("Common.Priority")]
        public string PriorityText { get; set; }

        [BaseEamResourceDisplayName("Common.Status")]
        public string Status { get; set; }

        [BaseEamResourceDisplayName("Site")]
        public long? SiteId { get; set; }
        public string SiteName { get; set; }

        [BaseEamResourceDisplayName("PurchaseRequest")]
        public long? PurchaseRequestId { get; set; }
        public string PurchaseRequestNumber { get; set; }

        [BaseEamResourceDisplayName("Requestor")]
        public long? RequestorId { get; set; }
        public string RequestorName { get; set; }

        [BaseEamResourceDisplayName("RequestForQuotation.ExpectedQuoteDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? ExpectedQuoteDate { get; set; }

        [BaseEamResourceDisplayName("RequestForQuotation.DateRequired")]
        [UIHint("DateTimeNullable")]
        public DateTime? DateRequired { get; set; }

        [BaseEamResourceDisplayName("Common.IsSent")]
        public bool IsSent { get; set; }

        [BaseEamResourceDisplayName("RequestForQuotation.ShipToAddress")]
        public long? ShipToAddressId { get; set; }
        [BaseEamResourceDisplayName("Common.Name")]
        public string ShipToAddressName { get; set; }
        [BaseEamResourceDisplayName("Address.Country")]
        public string ShipToAddressCountry { get; set; }
        [BaseEamResourceDisplayName("Address.StateProvince")]
        public string ShipToAddressStateProvince { get; set; }
        [BaseEamResourceDisplayName("Address.City")]
        public string ShipToAddressCity { get; set; }
        [BaseEamResourceDisplayName("Address.Address1")]
        public string ShipToAddressAddress1 { get; set; }
        [BaseEamResourceDisplayName("Address.Address2")]
        public string ShipToAddressAddress2 { get; set; }
        [BaseEamResourceDisplayName("Address.ZipPostalCode")]
        public string ShipToAddressZipPostalCode { get; set; }
        [BaseEamResourceDisplayName("Address.PhoneNumber")]
        public string ShipToAddressPhoneNumber { get; set; }
        [BaseEamResourceDisplayName("Address.FaxNumber")]
        public string ShipToAddressFaxNumber { get; set; }
        [BaseEamResourceDisplayName("Address.Email")]
        public string ShipToAddressEmail { get; set; }

        /// <summary>
        /// Cache available actions from assignment
        /// </summary>
        public string AvailableActions { get; set; }

        [BaseEamResourceDisplayName("Common.AssignedUsers")]
        public string AssignedUsers { get; set; }

        public string Comment { get; set; }
        public string ActionName { get; set; }
    }
}