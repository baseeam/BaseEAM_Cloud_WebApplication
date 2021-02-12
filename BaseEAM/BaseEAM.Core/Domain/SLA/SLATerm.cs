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
    public class SLATerm : BaseEntity
    {
        public long? SLADefinitionId { get; set; }
        public virtual SLADefinition SLADefinition { get; set; }

        /// <summary>
        /// The field of entity used as base to calculate the elapsed time
        /// It can be an entity's property or a FLEEExpression that has format {Status}DateTime
        /// </summary>
        public string TrackingBaseField { get; set; }

        /// <summary>
        /// The actual tracking field
        /// It can be an entity's property or a FLEEExpression that has format {Status}DateTime
        /// </summary>
        public string TrackingField { get; set; }

        /// <summary>
        /// Limit time in hours
        /// </summary>
        public int LimitHours { get; set; }

        /// <summary>
        /// Limit time in minutes
        /// </summary>
        public int LimitMinutes { get; set; }

        private ICollection<NotificationSequence> _notificationSequences;
        public virtual ICollection<NotificationSequence> NotificationSequences
        {
            get { return _notificationSequences ?? (_notificationSequences = new List<NotificationSequence>()); }
            protected set { _notificationSequences = value; }
        }
    }
}
