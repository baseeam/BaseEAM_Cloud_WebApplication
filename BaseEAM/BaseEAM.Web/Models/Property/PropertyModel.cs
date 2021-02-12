/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Mvc;
using BaseEAM.Web.Validators;
using FluentValidation.Attributes;

namespace BaseEAM.Web.Models
{
    [Validator(typeof(PropertyValidator))]
    public class PropertyModel : BaseEamEntityModel
    {
        [BaseEamResourceDisplayName("Common.Name")]
        public string Name { get; set; }

        [BaseEamResourceDisplayName("Site")]
        public long? SiteId { get; set; }
        public string SiteName { get; set; }

        [BaseEamResourceDisplayName("Location")]
        public long? LocationId { get; set; }
        public string LocationName { get; set; }

        [BaseEamResourceDisplayName("Common.Picture")]
        public long? PictureId { get; set; }
    }
}