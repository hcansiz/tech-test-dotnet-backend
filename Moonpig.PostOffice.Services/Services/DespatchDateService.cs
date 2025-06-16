using System;
using System.Collections.Generic;
using System.Linq;
using Moonpig.PostOffice.Data;
using Moonpig.PostOffice.Services.Interfaces;

namespace Moonpig.PostOffice.Services
{
    public class DespatchDateService : IDespatchDateService
    {
        private readonly DbContext _dbContext;

        public DespatchDateService()
        {
            _dbContext = new DbContext();
        }

        /// <summary>
        /// Calculates the despatch date for an order based on the products and their suppliers' lead times.
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="orderDate"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public DateTime CalculateDespatchDate(List<int> productIds, DateTime orderDate)
        {
            if (productIds == null || !productIds.Any())
                throw new ArgumentException("At least one product must be specified", nameof(productIds));

            var maxLeadTime = GetMaxLeadTime(productIds, orderDate);
            return HandleMoonpigWeekendClosure(maxLeadTime);
        }

        /// <summary>
        /// Gets the maximum lead time for the products in the order.
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="orderDate"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private DateTime GetMaxLeadTime(List<int> productIds, DateTime orderDate)
        {
            DateTime maxLeadTime = orderDate;

            foreach (var productId in productIds)
            {
                var product = _dbContext.Products.SingleOrDefault(x => x.ProductId == productId);
                if (product == null)
                    throw new ArgumentException($"Product with ID {productId} is not available", nameof(productIds));

                var supplier = _dbContext.Suppliers.SingleOrDefault(x => x.SupplierId == product.SupplierId);
                if (supplier == null)
                    throw new ArgumentException($"Product with ID {productId} is currently unavailable", nameof(productIds));

                var leadTime = supplier.LeadTime;
                var productArrivalDate = CalculateSupplierArrivalDate(orderDate, leadTime);

                if (productArrivalDate > maxLeadTime)
                    maxLeadTime = productArrivalDate;
            }

            return maxLeadTime;
        }

        /// <summary>
        /// Calculates the supplier's arrival date based on the order date and lead time.
        /// </summary>
        /// <param name="orderDate"></param>
        /// <param name="leadTime"></param>
        /// <returns></returns>
        private DateTime CalculateSupplierArrivalDate(DateTime orderDate, int leadTime)
        {
            var fullWeeks = leadTime / 5;
            var remainingDays = leadTime % 5;
            
            var resultDate = orderDate.AddDays(fullWeeks * 7);
            
            while (remainingDays > 0)
            {
                resultDate = resultDate.AddDays(1);
                if (resultDate.DayOfWeek != DayOfWeek.Saturday && 
                    resultDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    remainingDays--;
                }
            }
            
            return resultDate;
        }

        /// <summary>
        /// Handles the weekend closure for Moonpig, ensuring that orders placed on weekends are adjusted to the next working day.
        /// </summary>
        /// <param name="maxLeadTime"></param>
        /// <returns></returns>
        private DateTime HandleMoonpigWeekendClosure(DateTime maxLeadTime)
        {
            return maxLeadTime.DayOfWeek switch
            {
                DayOfWeek.Saturday => maxLeadTime.AddDays(2), // Saturday -> Monday
                DayOfWeek.Sunday => maxLeadTime.AddDays(1),   // Sunday -> Monday
                _ => maxLeadTime                              // Weekday, no adjustment needed
            };
        }
    }
}