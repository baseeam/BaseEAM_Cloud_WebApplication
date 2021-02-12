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
    public class SLADefinition : BaseEntity
    {
        public string Description { get; set; }

        public string EntityType { get; set; }

        private ICollection<SLATerm> _slaTerms;
        public virtual ICollection<SLATerm> SLATerms
        {
            get { return _slaTerms ?? (_slaTerms = new List<SLATerm>()); }
            protected set { _slaTerms = value; }
        }
    }
}
