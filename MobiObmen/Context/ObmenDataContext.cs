using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiObmen.Context
{
    class ObmenDataContext : DbContext
    {
        public ObmenDataContext()
            : base(ConfigurationManager.AppSettings["MSSQL"])
        {
        }
        public virtual DbSet<Models.RatePlan> RatePlans { get; set; }
        public virtual DbSet<Models.Request> Requests { get; set; }
    }
}
