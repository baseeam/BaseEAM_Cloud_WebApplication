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
    public class SLAInstanceModel : BaseEamEntityModel
    {
        public long? EntityId { get; set; }

        [BaseEamResourceDisplayName("Common.EntityType")]
        public string EntityType { get; set; }

        [BaseEamResourceDisplayName("SLADefinition")]
        public long? SLADefinitionId { get; set; }

        [BaseEamResourceDisplayName("Common.Violated")]
        public bool Violated { get; set; }
    }
}