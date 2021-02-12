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
    [Validator(typeof(TenantValidator))]
    public class TenantModel : BaseEamEntityModel
    {
        [BaseEamResourceDisplayName("Common.Name")]
        public string Name { get; set; }

        [BaseEamResourceDisplayName("Address")]
        public long? AddressId { get; set; }
        public AddressModel Address { get; set; }
    }
}