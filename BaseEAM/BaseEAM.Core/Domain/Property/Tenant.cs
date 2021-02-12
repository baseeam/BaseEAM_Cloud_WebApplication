/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System.Collections.Generic;

namespace BaseEAM.Core.Domain
{
    public class Tenant : BaseEntity
    {
        public long? AddressId { get; set; }
        public virtual Address Address { get; set; }

        public long? UserId { get; set; }
        public virtual User User { get; set; }

        private ICollection<Contact> _contacts;
        public virtual ICollection<Contact> Contacts
        {
            get { return _contacts ?? (_contacts = new List<Contact>()); }
            protected set { _contacts = value; }
        }

        private ICollection<TenantLease> _tenantLeases;
        public virtual ICollection<TenantLease> TenantLeases
        {
            get { return _tenantLeases ?? (_tenantLeases = new List<TenantLease>()); }
            protected set { _tenantLeases = value; }
        }

        private ICollection<TenantPayment> _tenantPayments;
        public virtual ICollection<TenantPayment> TenantPayments
        {
            get { return _tenantPayments ?? (_tenantPayments = new List<TenantPayment>()); }
            protected set { _tenantPayments = value; }
        }
    }
}
