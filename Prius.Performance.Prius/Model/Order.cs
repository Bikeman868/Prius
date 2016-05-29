using System;
using Prius.Performance.Shared;
using Prius.Contracts.Interfaces;

namespace Prius.Performance.Prius.Model
{
    public class Order: IOrder, IDataContract<Order>
    {
        public long OrderId { get; set; }
        public long CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal InvoiceTotal { get; set; }

        public void AddMappings(ITypeDefinition<Order> typeDefinition, string dataSetName)
        {
            typeDefinition.AddField("OrderID", o => o.OrderId, 0);
            typeDefinition.AddField("CustomerID", o => o.CustomerId, 0);
            typeDefinition.AddField("OrderDate", o => o.OrderDate, DateTime.UtcNow);
            typeDefinition.AddField("ShippedDate", o => o.ShippedDate, null);
            typeDefinition.AddField("OrderTotal", o => o.OrderTotal, 0);
            typeDefinition.AddField("TaxAmount", o => o.TaxAmount, 0);
            typeDefinition.AddField("ShippingAmount", o => o.ShippingAmount, 0);
        }

        public void SetCalculated(IDataReader dataReader, string dataSetName)
        {
            InvoiceTotal = OrderTotal + TaxAmount + ShippingAmount;
        }
    }
}
