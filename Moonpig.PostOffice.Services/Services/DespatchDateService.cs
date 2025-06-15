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

        public DateTime CalculateDespatchDate(List<int> productIds, DateTime orderDate)
        {
            if (productIds == null || !productIds.Any())
                throw new ArgumentException("At least one product must be specified", nameof(productIds));

            var maxLeadTime = GetMaxLeadTime(productIds, orderDate);

            return HandleMoonpigWeekendClosure(maxLeadTime);
        }

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

                if (orderDate.AddDays(leadTime) > maxLeadTime)
                    maxLeadTime = orderDate.AddDays(leadTime);
            }

            return maxLeadTime;
        }

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