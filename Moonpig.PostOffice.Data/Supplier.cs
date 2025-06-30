using System.Collections.Generic;
using System.Linq;

namespace Moonpig.PostOffice.Data
{
    public class Supplier
    {
        public int SupplierId { get; set; }


        public string Name { get; set; }


        public int LeadTime { get; set; }

        public List<SupplierBlockedDay> SupplierBlockedDays { get; set; }
    }
}
