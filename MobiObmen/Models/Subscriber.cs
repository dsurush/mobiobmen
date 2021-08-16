using log4net;
using MobiObmen.Boss4ECareInterfaceService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MobiObmen.ServiceReference1;
using MobiObmen.UnifiedInterfaceService;
using MobiObmen.Controllers;
using MobiObmen.Services;

namespace MobiObmen.Models
{
    public class Subscriber
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Subscriber));
        private static readonly Dictionary<string, string> resourcesAnalog = new Dictionary<string, string>
        {
            {"5050", "4224"}, //SMS
            {"4500", "4504"}, //MB
            {"5001", "4094"}  //Min
        };

        public static string GetSubscriberId(string msisdn)
        {
            try
            {
                var svc = new Boss4ECareInterfaceService.Boss4ECareInterfaceService();
                var qry = new Boss4ECareInterfaceService.getSubscribers
                {
                    AccessSessionRequest = new Boss4ECareInterfaceService.AccessSessionValue
                    {
                        accessChannel = "3",
                        mvnoId = "999",
                        operatorCode = "ecare",
                        password = "ecare"
                    },
                    GetSubscribersRequest = new Boss4ECareInterfaceService.QuerySubscirberConditionValue()
                };
                qry.GetSubscribersRequest.msisdn = msisdn;
                var response = svc.getSubscribers(qry);

                if (response.ResultOfOperationReply.resultCode == "0")
                {
                    return response.GetSubscribersReply[0].subscriberId;
                }
                return null;
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
                return null;
            }
        }
        public static Boss4ECareInterfaceService.FreeUnitsInfoValuesFreeUnitsInfoValue[] SubscriberInfo(string msisdn)
        {
            //Console.WriteLine(GetSubscriberId(msisdn));
            var svc = new Boss4ECareInterfaceService.Boss4ECareInterfaceService();
            var qry = new Boss4ECareInterfaceService.getFreeUnits
            {
                AccessSessionRequest = new Boss4ECareInterfaceService.AccessSessionValue
                {
                    accessChannel = "3",
                    mvnoId = "999",
                    operatorCode = "ecare",
                    password = "ecare"
                },
                GetFreeUnitsRequest = new Boss4ECareInterfaceService.QueryFreeUnitsConditionValue()
            };

            try
            {
                qry.GetFreeUnitsRequest.subscriberID = GetSubscriberId(msisdn);
                GetRatePlan(qry.GetFreeUnitsRequest.subscriberID);
                Boss4ECareInterfaceService.getFreeUnitsResponse resp = svc.getFreeUnits(qry);

                if (resp.ResultOfOperationReply.resultCode == "0")
                {
                    // Console.WriteLine(JsonConvert.SerializeObject(resp.GetFreeUnitsReply));
                    return resp.GetFreeUnitsReply;
                }
                return null;
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
                return null;
            }
        }
        public static Boss4ECareInterfaceService.SubscriberProductValue[] GetRatePlan(string subscriberId)
        {
            var svc = new Boss4ECareInterfaceService.Boss4ECareInterfaceService();
            var qry = new Boss4ECareInterfaceService.getUsingProducts
            {
                AccessSessionRequest = new Boss4ECareInterfaceService.AccessSessionValue
                {
                    accessChannel = "3",
                    mvnoId = "999",
                    operatorCode = "ecare",
                    password = "ecare"
                },
                GetUsingProductsRequest = new Boss4ECareInterfaceService.QueryBySubIdValue
                {
                    subId = subscriberId
                }
            };
            try
            {
                var response = svc.getUsingProducts(qry);
                if (response.ResultOfOperationReply.resultCode == "0")
                {
                    //  Console.WriteLine(JsonConvert.SerializeObject(response.GetUsingProductsReply));
                    return response.GetUsingProductsReply;
                }
                return null;
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
                return null;
            }
        }
        public static async Task<bool> CheckRatePlanAsync(string subId)
        {
            try
            {
                var PermittedRatePlans = await Services.DataBase.GetPlans();
                if (PermittedRatePlans == null)
                {
                    throw new Exception("Can't Get from DB RatePlans");
                }
                var RatePlanbySubId = GetRatePlan(subId);
                if (RatePlanbySubId == null)
                {
                    throw new Exception("Can't Get from CRM RatePlan by id");
                }
                return RatePlanbySubId.Any(plan => PermittedRatePlans.Any(permitedPlan => plan.productId == permitedPlan.PlanProductId));
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
                return false;
            }
        }
        public static bool CheckBalance(string Balance)
        {
            try
            {
                if (Int32.Parse(Balance) < 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
                return false;
            }
        }
        public static bool CheckHasQuantityResource(string QuantityResource, Models.Resource hasResource, string ResourceCode)
        {
            try
            {
                switch (ResourceCode)
                {
                    case "5050":
                        return (Int32.Parse(QuantityResource) <= Int32.Parse(hasResource.SMS));
                    case "4500":
                        return (Int32.Parse(QuantityResource) <= Int32.Parse(hasResource.GPRS));
                    case "5001":
                        return (Int32.Parse(QuantityResource) <= Int32.Parse(hasResource.Min));
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
                return false;
            }
        }
        public static bool DoExchange(ResourceExchangeRequest request)
        {
            /*
            {"5050", "4224"}, //SMS
            {"4500", "4504"}, //MB
            {"5001", "4094"}  //Min
             */
            try
            {
                var analogOfResource = resourcesAnalog[request.ToResource];
                var hasTransfer = false;
                var hasAddPacket = false;
                switch (request.Resource)
                {
                    case "5001": //Min
                        {
                            hasTransfer = TransferPacket(request.MSISDN, "918683429", request.QuantityResource, request.Resource);
                            if (!hasTransfer)
                            {
                                _log.Error("Error:" + $"Can't transfer packet by number {request.MSISDN}");
                                return false;
                            }
                            var quantityOfExchangeResource = (Int32.Parse(request.QuantityResource) * 10).ToString();
                            hasAddPacket = Models.Subscriber.AddMobiPackage(request.MSISDN, "", quantityOfExchangeResource, analogOfResource);
                            //TODO DELETE
                            hasAddPacket = false;
                            if (!hasAddPacket)
                            {
                                _log.Error("Error:" + "Can't add Package");
                                MailSender.SendEmail(request, quantityOfExchangeResource);
                                return false;
                            }
                            return true;
                        }
                    case "5050": //SMS
                        {
                            var roundQuantityResource = (Int32.Parse(request.QuantityResource) - (Int32.Parse(request.QuantityResource) % 30)).ToString();
                            hasTransfer = TransferPacket(request.MSISDN, "918683429", roundQuantityResource, request.Resource);
                            if (!hasTransfer)
                            {
                                _log.Error("Error:" + "Can't transfer packet");
                                return false;
                            }
                            var quantityOfExchangeResource = (Int32.Parse(request.QuantityResource) / 30).ToString();
                            hasAddPacket = Models.Subscriber.AddMobiPackage(request.MSISDN, "", quantityOfExchangeResource, analogOfResource);
                            if (!hasAddPacket)
                            {
                                _log.Error("Error:" + "Can't add Package");
                                MailSender.SendEmail(request, quantityOfExchangeResource);
                                return false;
                            }
                            return true;
                        }
                    case "4500": //MB
                        {
                            if (request.ToResource == "5001")
                            {
                                var roundQuantityResource = (Int32.Parse(request.QuantityResource) - (Int32.Parse(request.QuantityResource) % 30)).ToString();
                                hasTransfer = TransferPacket(request.MSISDN, "918683429", roundQuantityResource, request.Resource);
                                if (!hasTransfer)
                                {
                                    _log.Error("Error:" + "Can't transfer packet");
                                    return false;
                                }
                                var quantityOfExchangeResource = (Int32.Parse(request.QuantityResource) / 30).ToString();
                                hasAddPacket = Models.Subscriber.AddMobiPackage(request.MSISDN, "", quantityOfExchangeResource, analogOfResource);
                                if (!hasAddPacket)
                                {
                                    _log.Error("Error:" + "Can't add Package");
                                    MailSender.SendEmail(request, quantityOfExchangeResource);
                                    return false;
                                }
                                return true;
                            }
                            if (request.ToResource == "5050")
                            {
                                hasTransfer = TransferPacket(request.MSISDN, "918683429", request.QuantityResource, request.Resource);
                                if (!hasTransfer)
                                {
                                    _log.Error("Error:" + "Can't transfer packet");
                                    return false;
                                }
                                var quantityOfExchangeResource = (Int32.Parse(request.QuantityResource) * 30).ToString();
                                hasAddPacket = Models.Subscriber.AddMobiPackage(request.MSISDN, "", quantityOfExchangeResource, analogOfResource);
                                if (!hasAddPacket)
                                {
                                    _log.Error("Error:" + "Can't add Package");
                                    MailSender.SendEmail(request, quantityOfExchangeResource);
                                    return false;
                                }
                                return true;
                            }
                            break;
                        }
                }
                return false;
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
                return false;
            }
        }
        public static Models.Resource GetSourceId(string msisdn)
        {
            try
            {
                Models.Resource SubResource = new Models.Resource();
                var SubInfos = SubscriberInfo(msisdn);
                foreach (var SubInfo in SubInfos)
                {
                    switch (SubInfo.fuTypeName)
                    {
                        case "5050":
                            SubResource.SMS = SubInfo.fuUsedAmount;
                            break;
                        case "4500":
                            SubResource.GPRS = SubInfo.fuUsedAmount;
                            break;
                        case "5001":
                            SubResource.Min = SubInfo.fuUsedAmount;
                            break;
                        case "3000":
                            SubResource.Balance = SubInfo.fuUsedAmount;
                            break;
                    }
                }
                return SubResource;
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
                return null;
            }
        }
        public static bool TransferPacket(string fromMsisdn, string toMsisdn, string quantity, string sourceId)
        {
            try
            {
                var interfaceService = new Boss4UnifiedInterfaceServicePortTypeClient("Boss4UnifiedInterfaceServiceSOAP12port_http");
                var req = new processBusi();
                req.Trade = new Trade { ParamList = new TradeParamList { operationID = "10065", Param = new TradeParamListParam[5] } };
                req.Trade.ParamList.Param[0] = new TradeParamListParam { name = "MSISDN", value = fromMsisdn };
                req.Trade.ParamList.Param[1] = new TradeParamListParam { name = "RecMSISDN", value = toMsisdn };
                req.Trade.ParamList.Param[2] = new TradeParamListParam { name = "AccountType", value = sourceId };
                req.Trade.ParamList.Param[3] = new TradeParamListParam { name = "Amount", value = quantity };
                req.Trade.ParamList.Param[4] = new TradeParamListParam { name = "AccessMethod", value = "12" };
                req.Trade.System = new TradeSystem();
                var response = interfaceService.processBusi(req);
                //              Console.WriteLine(JsonConvert.SerializeObject(response.Trade));
                //              Console.WriteLine("response.Trade.ParamList.retCode = " + response.Trade.ParamList.retCode);
                if (response.Trade.ParamList.retCode == "0")
                {
                    return true;
                }
                _log.Error($"TransferPacket from {fromMsisdn} to {toMsisdn} " + response.Trade.ParamList.desc + $" retCode = {response.Trade.ParamList.retCode}");
                return false;
            }
            catch (Exception exp)
            {
                _log.Error($"TransferPacket from {fromMsisdn} to {toMsisdn}" + "Error:", exp);
                return false;
            }

        }
        public static bool AddMobiPackage(string fromMsisdn, string toMsisdn, string quantity, string sourceId)
        {
            try
            {
                var interfaceService = new Boss4UnifiedInterfaceServicePortTypeClient("Boss4UnifiedInterfaceServiceSOAP12port_http");
                var req = new processBusi();
                req.Trade = new Trade();
                req.Trade.ParamList = new TradeParamList();
                string opCode = "10046";
                req.Trade.ParamList.operationID = opCode;
                req.Trade.ParamList.Param = new TradeParamListParam[6];
                req.Trade.ParamList.Param[0] = new TradeParamListParam();

                req.Trade.ParamList.Param[0].name = "AccessMethod";
                req.Trade.ParamList.Param[0].value = "12";

                req.Trade.ParamList.Param[1] = new TradeParamListParam();
                req.Trade.ParamList.Param[1].name = "MSISDN";
                req.Trade.ParamList.Param[1].value = fromMsisdn;

                req.Trade.ParamList.Param[2] = new TradeParamListParam();
                req.Trade.ParamList.Param[2].name = "UsedMsisdn";
                req.Trade.ParamList.Param[2].value = toMsisdn;

                req.Trade.ParamList.Param[3] = new TradeParamListParam();
                req.Trade.ParamList.Param[3].name = "ResTypeId";
                req.Trade.ParamList.Param[3].value = "15";

                req.Trade.ParamList.Param[4] = new TradeParamListParam();
                req.Trade.ParamList.Param[4].name = "ResModelId";
                req.Trade.ParamList.Param[4].value = sourceId;

                req.Trade.ParamList.Param[5] = new TradeParamListParam();
                req.Trade.ParamList.Param[5].name = "Amount";
                req.Trade.ParamList.Param[5].value = quantity;

                req.Trade.System = new TradeSystem();
                var response = interfaceService.processBusi(req);

                //  Console.WriteLine("response.Trade.ParamList.retCode = " + response.Trade.ParamList.retCode);
                //   Console.WriteLine("response.Trade.ParamList.desc = " + response.Trade.ParamList.desc);
                if (response.Trade.ParamList.retCode == "0")
                {
                    return true;
                }
                _log.Error($"AddMobiPackage from {fromMsisdn}" + response.Trade.ParamList.desc + $" retCode = {response.Trade.ParamList.retCode}");
                return false;
            }
            catch (Exception exp)
            {
                _log.Error(exp.Message);
                return false;
            }

        }
    }
}
