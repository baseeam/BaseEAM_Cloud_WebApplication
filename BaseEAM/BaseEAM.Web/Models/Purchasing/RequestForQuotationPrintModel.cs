/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Web.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaseEAM.Web.Models
{
    public class RequestForQuotationPrintModel
    {
        public long Id { get; set; }

        [BaseEamResourceDisplayName("Common.Number")]
        public string Number { get; set; }

        [BaseEamResourceDisplayName("RequestForQuotation.ExpectedQuoteDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? ExpectedQuoteDate { get; set; }

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

        public List<RequestForQuotationItemModel> RequestForQuotationItems { get; set; }
    }
}