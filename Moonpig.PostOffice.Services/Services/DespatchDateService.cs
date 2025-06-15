using System;
using System.Collections.Generic;
using System.Linq;
using Moonpig.PostOffice.Data;
using Moonpig.PostOffice.Services.Interfaces;

namespace Moonpig.PostOffice.Services
{
    public class DespatchDateService : IDespatchDateService
    {
        public DateTime CalculateDespatchDate(List<int> productIds, DateTime orderDate)
        {
            DateTime maxLeadTime = orderDate;
            
            foreach (var productId in productIds)
            {
                DbContext dbContext = new DbContext();
                var supplierId = dbContext.Products.Single(x => x.ProductId == productId).SupplierId;
                var leadTime = dbContext.Suppliers.Single(x => x.SupplierId == supplierId).LeadTime;
                
                if (orderDate.AddDays(leadTime) > maxLeadTime)
                    maxLeadTime = orderDate.AddDays(leadTime);
            }
            
            if (maxLeadTime.DayOfWeek == DayOfWeek.Saturday)
            {
                return maxLeadTime.AddDays(2);
            }
            else if (maxLeadTime.DayOfWeek == DayOfWeek.Sunday)
            {
                return maxLeadTime.AddDays(1);
            }
            else
            {
                return maxLeadTime;
            }
        }
    }
}