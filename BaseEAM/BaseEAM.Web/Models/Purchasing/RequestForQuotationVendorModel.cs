/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Mvc;
using BaseEAM.Web.Validators;
using FluentValidation.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace BaseEAM.Web.Models
{
    [Validator(typeof(RequestForQuotationVendorValidator))]
    public class RequestForQuotationVendorModel : BaseEamEntityModel
    {
        public long? RequestForQuotationId { get; set; }

        [BaseEamResourceDisplayName("Vendor")]
        public long? VendorId { get; set; }
        public string VendorName { get; set; }

        [BaseEamResourceDisplayName("Contact.Name")]
        public string VendorContactName { get; set; }

        [BaseEamResourceDisplayName("Contact.Email")]
        public string VendorContactEmail { get; set; }

        [BaseEamResourceDisplayName("Contact.Phone")]
        public string VendorContactPhone { get; set; }

        [BaseEamResourceDisplayName("Contact.Fax")]
        public string VendorContactFax { get; set; }

        [BaseEamResourceDisplayName("RequestForQuotationVendor.VendorQuoteNumber")]
        public string VendorQuoteNumber { get; set; }

        [BaseEamResourceDisplayName("RequestForQuotationVendor.VendorQuoteDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? VendorQuoteDate { get; set; }
    }
}