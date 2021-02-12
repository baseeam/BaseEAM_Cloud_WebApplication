/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Mvc;
using BaseEAM.Web.Validators;
using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseEAM.Web.Models
{
    [Validator(typeof(NotificationSequenceValidator))]
    public class NotificationSequenceModel
    {
        public long Id { get; set; }

        public long? SLATermId { get; set; }

        [BaseEamResourceDisplayName("Common.Sequence")]
        public int Sequence { get; set; }

        [BaseEamResourceDisplayName("NotificationSequence.SendingTimeHours")]
        public int SendingTimeHours { get; set; }

        [BaseEamResourceDisplayName("NotificationSequence.SendingTimeMinutes")]
        public int SendingTimeMinutes { get; set; }

        [BaseEamResourceDisplayName("Common.Users")]
        public string Users { get; set; }

        [BaseEamResourceDisplayName("MessageTemplate")]
        public string MessageTemplate { get; set; }
    }
}