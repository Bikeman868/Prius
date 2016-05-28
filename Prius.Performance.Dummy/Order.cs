using System;
using Prius.Performance.Shared;

namespace Prius.Performance.Dummy
{
    public class Order: IOrder
    {
        public long OrderId { get; set; }
        public long CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmmount { get; set; }
        public decimal InvoiceTotal { get; set; }
    }
}
