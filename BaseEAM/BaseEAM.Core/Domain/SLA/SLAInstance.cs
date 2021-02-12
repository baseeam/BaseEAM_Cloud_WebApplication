/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseEAM.Core.Domain
{
    public class SLAInstance : BaseEntity
    {
        public long? EntityId { get; set; }
        public string EntityType { get; set; }

        public long? SLADefinitionId { get; set; }
        public virtual SLADefinition SLADefinition { get; set; }

        public bool Violated { get; set; }
        public bool Closed { get; set; }

        private ICollection<SLAInstanceTerm> _slaInstanceTerms;
        public virtual ICollection<SLAInstanceTerm> SLAInstanceTerms
        {
            get { return _slaInstanceTerms ?? (_slaInstanceTerms = new List<SLAInstanceTerm>()); }
            protected set { _slaInstanceTerms = value; }
        }
    }
}
