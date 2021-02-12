/*******************************************************
 * Copyright 2016 (C) BaseEAM Systems, Inc
 * All Rights Reserved
*******************************************************/
using BaseEAM.Web.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaseEAM.Web.Models
{
    public class PurchaseOrderPrintModel
    {
        public long Id { get; set; }

        [BaseEamResourceDisplayName("Common.Number")]
        public string Number { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrder.PaymentTerm")]
        public long? PaymentTermId { get; set; }
        public string PaymentTermName { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrder.ExpectedDeliveryDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrder.DateOrdered")]
        [UIHint("DateTimeNullable")]
        public DateTime? DateOrdered { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrder.ShipToAddress")]
        public long? ShipToAddressId { get; set; }
        [BaseEamResourceDisplayName("Common.Name")]
        public string ShipToAddressName { get; set; }
        [BaseEamResourceDisplayName("Address.Country")]
        public string ShipToAddressCountry { get; set; }
        [BaseEamResourceDisplayName("Address.StateProvince")]
        public string ShipToAddressStateProvince { get; set; }
        [BaseEamResourceDisplayName("Address.City")]
        public string ShipToAddressCity { get; set; }
        [BaseEamResourceDisplayName("Address.Address1")]
        public string ShipToAddressAddress1 { get; set; }
        [BaseEamResourceDisplayName("Address.Address2")]
        public string ShipToAddressAddress2 { get; set; }
        [BaseEamResourceDisplayName("Address.ZipPostalCode")]
        public string ShipToAddressZipPostalCode { get; set; }
        [BaseEamResourceDisplayName("Address.PhoneNumber")]
        public string ShipToAddressPhoneNumber { get; set; }
        [BaseEamResourceDisplayName("Address.FaxNumber")]
        public string ShipToAddressFaxNumber { get; set; }
        [BaseEamResourceDisplayName("Address.Email")]
        public string ShipToAddressEmail { get; set; }

        [BaseEamResourceDisplayName("PurchaseOrder.BillToAddress")]
        public long? BillToAddressId { get; set; }
        [BaseEamResourceDisplayName("Common.Name")]
        public string BillToAddressName { get; set; }
        [BaseEamResourceDisplayName("Address.Country")]
        public string BillToAddressCountry { get; set; }
        [BaseEamResourceDisplayName("Address.StateProvince")]
        public string BillToAddressStateProvince { get; set; }
        [BaseEamResourceDisplayName("Address.City")]
        public string BillToAddressCity { get; set; }
        [BaseEamResourceDisplayName("Address.Address1")]
        public string BillToAddressAddress1 { get; set; }
        [BaseEamResourceDisplayName("Address.Address2")]
        public string BillToAddressAddress2 { get; set; }
        [BaseEamResourceDisplayName("Address.ZipPostalCode")]
        public string BillToAddressZipPostalCode { get; set; }
        [BaseEamResourceDisplayName("Address.PhoneNumber")]
        public string BillToAddressPhoneNumber { get; set; }
        [BaseEamResourceDisplayName("Address.FaxNumber")]
        public string BillToAddressFaxNumber { get; set; }
        [BaseEamResourceDisplayName("Address.Email")]
        public string BillToAddressEmail { get; set; }

        public List<PurchaseOrderItemModel> PurchaseOrderItems { get; set; }

        public List<PurchaseOrderMiscCostModel> PurchaseOrderMiscCosts { get; set; }

        public decimal? Subtotal
        {
            get
            {
                decimal? total = 0;
                foreach (var item in this.PurchaseOrderItems)
                {
                    total += (item.Subtotal ?? 0);
                }
                return total;
            }
        }

        public decimal? SubtotalWithTax
        {
            get
            {
                decimal? total = 0;
                foreach (var item in this.PurchaseOrderItems)
                {
                    total += (item.SubtotalWithTax ?? 0);
                }
                return total;
            }
        }

        public decimal? Total
        {
            get
            {
                decimal? total = 0;
                foreach (var item in this.PurchaseOrderItems)
                {
                    total += (item.Subtotal ?? 0);
                }
                foreach (var item in this.PurchaseOrderMiscCosts)
                {
                    total += (item.Amount ?? 0);
                }
                return total;
            }
        }

        public decimal? TotalWithTax
        {
            get
            {
                decimal? total = 0;
                foreach (var item in this.PurchaseOrderItems)
                {
                    total += (item.SubtotalWithTax ?? 0);
                }
                foreach (var item in this.PurchaseOrderMiscCosts)
                {
                    total += (item.Amount ?? 0);
                }
                return total;
            }
        }
    }
}