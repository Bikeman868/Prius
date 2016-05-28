using System;
using System.Collections.Generic;

namespace Prius.Performance.Shared
{
    public interface ICustomer
    {
        long CustomerId { get; }
        string GivenNames { get; set; }
        string FamilyName { get; set; }
        DateTime DateOfBirth { get; set; }
        string Email { get; set; }

        IList<IOrder> Orders { get; }
    }

    public static class CustomerExtensions
    {
        public static int CalculateAge(this ICustomer customer, DateTime? date = null)
        {
            var today = date ?? DateTime.UtcNow;
            int age = today.Year - customer.DateOfBirth.Year;

            if (customer.DateOfBirth > today.AddYears(-age))
                age--;

            return age;
        }
    }
}
