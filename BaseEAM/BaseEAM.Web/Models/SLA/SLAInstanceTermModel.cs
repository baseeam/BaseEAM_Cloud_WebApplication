/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseEAM.Web.Models
{
    public class SLAInstanceTermModel : BaseEamEntityModel
    {
        public long? SLAInstanceId { get; set; }

        public long? SLATermId { get; set; }

        [BaseEamResourceDisplayName("SLATerm")]
        public string SLATermName { get; set; }

        [BaseEamResourceDisplayName("SLATerm.TrackingBaseField")]
        public string SLATermTrackingBaseField { get; set; }

        [BaseEamResourceDisplayName("SLATerm.TrackingField")]
        public string SLATermTrackingField { get; set; }

        [BaseEamResourceDisplayName("SLATerm.LimitHours")]
        public int SLATermLimitHours { get; set; }

        [BaseEamResourceDisplayName("SLATerm.LimitMinutes")]
        public int SLATermLimitMinutes { get; set; }

        [BaseEamResourceDisplayName("SLAInstanceTerm.TrackingBaseDateTime")]
        public DateTime? TrackingBaseDateTime { get; set; }

        [BaseEamResourceDisplayName("SLAInstanceTerm.TrackingDateTime")]
        public DateTime? TrackingDateTime { get; set; }

        [BaseEamResourceDisplayName("Common.Violated")]
        public bool Violated { get; set; }
    }
}