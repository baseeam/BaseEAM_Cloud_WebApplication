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
    [Validator(typeof(SLADefinitionValidator))]
    public class SLADefinitionModel : BaseEamEntityModel
    {
        [BaseEamResourceDisplayName("Common.Name")]
        public string Name { get; set; }

        [BaseEamResourceDisplayName("Common.Description")]
        public string Description { get; set; }

        [BaseEamResourceDisplayName("EntityType")]
        public string EntityType { get; set; }
    }
}