/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Mvc;
using BaseEAM.Web.Validators;
using FluentValidation.Attributes;
using System;
using System.Web.Mvc;

namespace BaseEAM.Web.Models
{
    [Validator(typeof(TenantPaymentCollectionValidator))]
    [Bind(Exclude = "PaymentMethod")]
    public class TenantPaymentCollectionModel : BaseEamEntityModel
    {
        public long? TenantPaymentId { get; set; }

        [BaseEamResourceDisplayName("TenantPaymentCollection.ReceivedDate")]
        public DateTime? ReceivedDate { get; set; }

        [BaseEamResourceDisplayName("TenantPaymentCollection.ReceivedAmount")]
        public decimal? ReceivedAmount { get; set; }

        [BaseEamResourceDisplayName("TenantPaymentCollection.CheckNum")]
        public string CheckNum { get; set; }

        [BaseEamResourceDisplayName("TenantPaymentCollection.PaymentMethod")]
        public long? PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; }

        public ValueItemModel PaymentMethod { get; set; }
    }
}