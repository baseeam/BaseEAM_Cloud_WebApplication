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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BaseEAM.Web.Models
{
    [Validator(typeof(SLATermValidator))]
    public class SLATermModel : BaseEamEntityModel
    {
        public long? SLADefinitionId { get; set; }

        [BaseEamResourceDisplayName("Common.Name")]
        public string Name { get; set; }

        [BaseEamResourceDisplayName("SLATerm.TrackingBaseField")]
        public string TrackingBaseField { get; set; }

        [BaseEamResourceDisplayName("SLATerm.TrackingField")]
        public string TrackingField { get; set; }

        [BaseEamResourceDisplayName("SLATerm.LimitHours")]
        [UIHint("Int32")]
        public int LimitHours { get; set; }

        [BaseEamResourceDisplayName("SLATerm.LimitMinutes")]
        [UIHint("Int32")]
        public int LimitMinutes { get; set; }
    }
}