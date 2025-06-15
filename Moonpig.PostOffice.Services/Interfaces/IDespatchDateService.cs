using System;
using System.Collections.Generic;

namespace Moonpig.PostOffice.Services.Interfaces
{
    public interface IDespatchDateService
    {
        DateTime CalculateDespatchDate(List<int> productIds, DateTime orderDate);
    }
}