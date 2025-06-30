using Microsoft.VisualBasic;
using System;

namespace Moonpig.PostOffice.Data
{
    public class SupplierBlockedDay
    {
        public int SupplierBlockedDayId { get; set; }

        public int SupplierId { get; set; }

        public DateTime Date { get; set; }
    }
}
