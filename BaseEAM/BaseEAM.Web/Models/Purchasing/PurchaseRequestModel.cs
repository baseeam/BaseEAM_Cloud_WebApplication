/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Core.Domain;
using BaseEAM.Web.Framework;
using BaseEAM.Web.Framework.Mvc;
using BaseEAM.Web.Validators;
using FluentValidation.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace BaseEAM.Web.Models
{
    [Validator(typeof(PurchaseRequestValidator))]
    public class PurchaseRequestModel : BaseEamEntityModel
    {
        [BaseEamResourceDisplayName("Common.Number")]
        public string Number { get; set; }

        [BaseEamResourceDisplayName("Common.Description")]
        public string Description { get; set; }

        public AssignmentPriority Priority { get; set; }
        [BaseEamResourceDisplayName("Common.Priority")]
        public string PriorityText { get; set; }

        [BaseEamResourceDisplayName("Common.Status")]
        public string Status { get; set; }

        [BaseEamResourceDisplayName("Site")]
        public long? SiteId { get; set; }
        public string SiteName { get; set; }

        [BaseEamResourceDisplayName("Requestor")]
        public long? RequestorId { get; set; }
        public string RequestorName { get; set; }

        [BaseEamResourceDisplayName("PurchaseRequest.DateRequired")]
        [UIHint("DateTimeNullable")]
        public DateTime? DateRequired { get; set; }

        /// <summary>
        /// Cache available actions from assignment
        /// </summary>
        public string AvailableActions { get; set; }

        [BaseEamResourceDisplayName("Common.AssignedUsers")]
        public string AssignedUsers { get; set; }

        public string Comment { get; set; }
        public string ActionName { get; set; }
    }
}