using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MobiObmen.Controllers
{
    public class TestController : ApiController 
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            var subID = Models.Subscriber.GetSubscriberId("918683429");
          //  var Answer = Models.Subscriber.GetRatePlan(subID);
          //  Console.WriteLine(Answer);
          //var Answer = Models.Subscriber.SubscriberInfo("918664573");
          //var Answer1 = Models.Subscriber.SubscriberInfo("918664573");
            List<Boss4ECareInterfaceService.FreeUnitsInfoValuesFreeUnitsInfoValue[]> Answer = new List<Boss4ECareInterfaceService.FreeUnitsInfoValuesFreeUnitsInfoValue[]>();
          //  Answer.Add(Answer2);
          //  Answer.Add(Answer1);
          // Models.Subscriber.TransferPacket("918683429", "918664573", "1", "5050");
           var Answer2 = Models.Subscriber.SubscriberInfo("918683429");
           var Answer1 = Models.Subscriber.SubscriberInfo("918664573");
            Answer.Add(Answer2);
            Answer.Add(Answer1);
          //var SubID = Models.Subscriber.GetSubscriberId("918683429");
          // var SubID = Models.Subscriber.GetSubscriberId("988703203");
          //   var Answer = Models.Subscriber.GetRatePlan(SubID);
            return Json(Answer);
        }
    }
}
