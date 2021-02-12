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
    public class SLAInstanceTerm : BaseEntity
    {
        public long? SLAInstanceId { get; set; }
        public virtual SLAInstance SLAInstance { get; set; }

        public long? SLATermId { get; set; }
        public virtual SLATerm SLATerm { get; set; }

        public DateTime? TrackingBaseDateTime { get; set; }
        public DateTime? TrackingDateTime { get; set; }

        public bool Violated { get; set; }
    }
}
