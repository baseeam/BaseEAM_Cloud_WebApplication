/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System;

namespace BaseEAM.Core.Domain
{
    public class TenantPaymentCollection : BaseEntity
    {
        public long? TenantPaymentId { get; set; }
        public virtual TenantPayment TenantPayment { get; set; }

        public DateTime? ReceivedDate { get; set; }
        public decimal? ReceivedAmount { get; set; }
        public string CheckNum { get; set; }
        public long? PaymentMethodId { get; set; }
        public virtual ValueItem PaymentMethod { get; set; }
    }
}
