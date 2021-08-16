using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiObmen.Models
{
    public class Request
    {
        [Key]
        public int ID { get; set; }
        public string MSISDN { get; set; }
        public string Resource { get; set; }
        public string QuantityResource { get; set; }
        public string ToResource { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int Status { get; set; }
    }
}
