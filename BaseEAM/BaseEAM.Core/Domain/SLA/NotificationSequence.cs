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
    public class NotificationSequence : BaseEntity
    {
        public long? SLATermId { get; set; }
        public virtual SLATerm SLATerm { get; set; }

        public int Sequence { get; set; }

        // Time to send notification = SLATerm.StartFrom + SendingTimeHours + SendingTimeMinutes
        public int SendingTimeHours { get; set; }
        public int SendingTimeMinutes { get; set; }

        // Send Message
        public string Users { get; set; }
        public string MessageTemplate { get; set; }
    }
}
