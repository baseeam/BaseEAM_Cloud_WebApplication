/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System.Collections.Generic;

namespace BaseEAM.Core.Domain
{
    public class Property : BaseEntity
    {
        public long? SiteId { get; set; }
        public virtual Site Site { get; set; }

        public long? LocationId { get; set; }
        public virtual Location Location { get; set; }

        private ICollection<TenantLease> _tenantLeases;
        public virtual ICollection<TenantLease> TenantLeases
        {
            get { return _tenantLeases ?? (_tenantLeases = new List<TenantLease>()); }
            protected set { _tenantLeases = value; }
        }
    }
}
