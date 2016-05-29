using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prius.Performance.Shared
{
    public interface IOrder
    {
        long OrderId { get; }
        long CustomerId { get; }
        DateTime OrderDate { get; set; }
        DateTime? ShippedDate { get; set; }
        Decimal OrderTotal { get; set; }
        Decimal TaxAmount { get; set; }
        Decimal ShippingAmount { get; set; }
        Decimal InvoiceTotal { get; set; }
    }
}
