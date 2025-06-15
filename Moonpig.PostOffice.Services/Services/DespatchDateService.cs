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
            DateTime maxLeadTime = orderDate;

            foreach (var productId in productIds)
            {
                var supplierId = _dbContext.Products.Single(x => x.ProductId == productId).SupplierId;
                var leadTime = _dbContext.Suppliers.Single(x => x.SupplierId == supplierId).LeadTime;

                if (orderDate.AddDays(leadTime) > maxLeadTime)
                    maxLeadTime = orderDate.AddDays(leadTime);
            }

            return HandleMoonpigWeekendClosure(maxLeadTime);
        }

        private DateTime HandleMoonpigWeekendClosure(DateTime arrivalDate)
        {
            return arrivalDate.DayOfWeek switch
            {
                DayOfWeek.Saturday => arrivalDate.AddDays(2), // Saturday -> Monday
                DayOfWeek.Sunday => arrivalDate.AddDays(1),   // Sunday -> Monday
                _ => arrivalDate                              // Weekday, no adjustment needed
            };
        }
    }
}