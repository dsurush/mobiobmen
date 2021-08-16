using log4net;
using MobiObmen.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiObmen.Services
{
    public class DataBase
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(DataBase));
        public async static Task<List<Models.RatePlan>> GetPlans()
        {
            try
            {
                var RatePlans = new List<Models.RatePlan>();
                using (var ctx = new Context.ObmenDataContext())
                {
                    RatePlans = await ctx.RatePlans.ToListAsync();
                }
                return RatePlans;
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
                return null;
            }
        }
        public async static Task<int> AddRequest(ResourceExchangeRequest request)
        {
            var newRequest = new Models.Request()
            {
                MSISDN = request.MSISDN,
                Resource = request.Resource,
                QuantityResource = request.QuantityResource,
                ToResource = request.ToResource,
                CreateDate = DateTime.Now,
                Status = 0,
            };
            try
            {
                using (var ctx = new Context.ObmenDataContext())
                {
                    ctx.Requests.Add(newRequest);
                    await ctx.SaveChangesAsync();
                }
                return newRequest.ID;
            }
            catch (Exception ex)
            {
                _log.Error($"Error: while AddRequest to DB {request.MSISDN} {request.QuantityResource} {request.Resource} {request.ToResource}", ex);
                return -1;
            }
        }
        public async static Task UpdateStatusRequestByID(int id)
        {
            try
            {
                using (var ctx = new Context.ObmenDataContext())
                {
                    var request = ctx.Requests.Find(id);
                    request.Status = 1;
                    request.UpdateDate = DateTime.Now;
                    await ctx.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error: while Update Request to DB with id = {id} ", ex);
            }
        }
    }
}
