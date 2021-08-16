using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiObmen.Models
{
    public class RatePlan
    {
        public int ID { get; set; }
        public string PlanProductId { get; set; }
        public string PlanProductName { get; set; }
    }
    public class Resource
    {
        public string Balance { get; set; }
        public string Min { get; set; }
        public string GPRS { get; set; }
        public string SMS { get; set; }
    }
}
