using log4net;
using MobiObmen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MobiObmen.Controllers
{
    public class ResourceExchangeController : ApiController
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(ResourceExchangeController));
        [HttpPost]
        public async Task<IHttpActionResult> Post(ResourceExchangeRequest request)
        {
            try
            {
                var ErrorMessage = new ErrorMessages();
                ErrorMessage.code = "0";
                ErrorMessage.message = "success";

                //ADD REQUEST TO DB
                var id = await Services.DataBase.AddRequest(request);
                if (id == -1)
                {
                    ErrorMessage.code = "7";
                    ErrorMessage.message = "Error with DB";
                    _log.Error("in ResourceExchangeController " + ErrorMessage.message + " " + request.MSISDN);
                    return Json(ErrorMessage);
                }
                ///Get SubID of MSISDN
                if (request.Resource == request.ToResource)
                {
                    ErrorMessage.code = "5";
                    ErrorMessage.message = "Exchange resources can't be equal";
                    _log.Error("in ResourceExchangeController " + ErrorMessage.message + " " + request.MSISDN);
                    return Json(ErrorMessage);
                }
                var subID = Models.Subscriber.GetSubscriberId(request.MSISDN);
                if (subID == null)
                {
                    ErrorMessage.code = "1";
                    ErrorMessage.message = "Wrong number or can't get SubID in CRM";
                    _log.Error("in ResourceExchangeController " + ErrorMessage.message + " " + request.MSISDN);
                    return Json(ErrorMessage);
                }
                ///Check RatePlan
                var hasRatePlan = await Models.Subscriber.CheckRatePlanAsync(subID);
                if (!hasRatePlan)
                {
                    ErrorMessage.code = "2";
                    ErrorMessage.message = "Your Rate Plan is wrong for this command";
                    _log.Error("in ResourceExchangeController " + ErrorMessage.message + " " + request.MSISDN);
                    return Json(ErrorMessage);
                }
                ///Check Balance
                var source = Models.Subscriber.GetSourceId(request.MSISDN);
                if (source == null)
                {
                    ErrorMessage.code = "3";
                    ErrorMessage.message = "Can't get source by msisdn";
                    _log.Error("in ResourceExchangeController " + ErrorMessage.message + " " + request.MSISDN);
                    return Json(ErrorMessage);
                }
                ///Check QResource and hasResource
                var hasResources = Models.Subscriber.CheckHasQuantityResource(request.QuantityResource, source, request.ToResource);
                if (!hasResources)
                {
                    ErrorMessage.code = "4";
                    ErrorMessage.message = "Not enough resources";
                    _log.Error("in ResourceExchangeController " + ErrorMessage.message + " " + request.MSISDN);
                    return Json(ErrorMessage);
                }
                var didExchange = Subscriber.DoExchange(request);
                if (!didExchange)
                {
                    ErrorMessage.code = "6";
                    ErrorMessage.message = "Can't do exchange";
                    _log.Error("in ResourceExchangeController " + ErrorMessage.message + " " + request.MSISDN);
                    return Json(ErrorMessage);
                }
                //UPDATE STATUS TO SUCCESS
                await Services.DataBase.UpdateStatusRequestByID(id);
                return Json(ErrorMessage);
            }
            catch (Exception exp)
            {
                _log.Error(exp.Message);
                return StatusCode(HttpStatusCode.BadRequest);
            }
        }
    }
    public class ResourceExchangeRequest
    {
        public string MSISDN { get; set; }
        public string Resource{ get; set; }
        public string QuantityResource { get; set; }
        public string ToResource { get; set; }
    }
}
