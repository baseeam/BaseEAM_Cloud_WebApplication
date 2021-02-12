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
    [Validator(typeof(PurchaseOrderValidator))]
    public class PurchaseOrderModel : BaseEamEntityModel
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

        [BaseEamResourceDisplayName("RequestForQuotation")]
        public long? RequestForQuotationId { get; set; }
        public string RequestForQuotationNumber { get; set; }

        [BaseEamResourceDisplayName("Requestor")]
        public long? RequestorId { get; set; }
        public string RequestorName { get; set; }

        [BaseEamResourceDisplayName("Supervisor")]
        public long? SupervisorId { get; set; }
        public string SupervisorName { get; set; }

        [BaseEamResourceDisplayName("Vendor")]
        public long? VendorId { get; set; }
        public string VendorName { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrder.PaymentTerm")]
        public long? PaymentTermId { get; set; }
        public string PaymentTermName { get; set; }

        [BaseEamResourceDisplayName("Common.IsSent")]
        public bool IsSent { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrder.ExpectedDeliveryDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrder.DateOrdered")]
        [UIHint("DateTimeNullable")]
        public DateTime? DateOrdered { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrder.DateRequired")]
        [UIHint("DateTimeNullable")]
        public DateTime? DateRequired { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrder.ShipToAddress")]
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

        [BaseEamResourceDisplayName("PurchaseOrder.BillToAddress")]
        public long? BillToAddressId { get; set; }
        [BaseEamResourceDisplayName("Common.Name")]
        public string BillToAddressName { get; set; }
        [BaseEamResourceDisplayName("Address.Country")]
        public string BillToAddressCountry { get; set; }
        [BaseEamResourceDisplayName("Address.StateProvince")]
        public string BillToAddressStateProvince { get; set; }
        [BaseEamResourceDisplayName("Address.City")]
        public string BillToAddressCity { get; set; }
        [BaseEamResourceDisplayName("Address.Address1")]
        public string BillToAddressAddress1 { get; set; }
        [BaseEamResourceDisplayName("Address.Address2")]
        public string BillToAddressAddress2 { get; set; }
        [BaseEamResourceDisplayName("Address.ZipPostalCode")]
        public string BillToAddressZipPostalCode { get; set; }
        [BaseEamResourceDisplayName("Address.PhoneNumber")]
        public string BillToAddressPhoneNumber { get; set; }
        [BaseEamResourceDisplayName("Address.FaxNumber")]
        public string BillToAddressFaxNumber { get; set; }
        [BaseEamResourceDisplayName("Address.Email")]
        public string BillToAddressEmail { get; set; }

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