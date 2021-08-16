using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MobiObmen.Controllers
{
    public class AddPackageController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            Models.Subscriber.AddMobiPackage("918664573", "", "1", "4504");
            return (Ok());
        }
    }
}
