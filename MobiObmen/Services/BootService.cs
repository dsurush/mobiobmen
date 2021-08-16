using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Routing;
using System.Web.Http.SelfHost;


namespace MobiObmen.Services
{
    internal class BootService
    {
       // private readonly ILog _log = LogManager.GetLogger(typeof(BootService));
        private HttpSelfHostServer _serv;

        private ServiceOrderJob _servJob;
        public BootService()
        {
            _servJob = new ServiceOrderJob();
            

        }


        private void StartSelfHost()
        {

            var selfHostConfiguraiton = new HttpSelfHostConfiguration("http://127.0.0.1:9088");
            selfHostConfiguraiton.Routes.MapHttpRoute(
               name: "DefaultApiRoute",
               routeTemplate: "{controller}",
               defaults: null
           );
            IHttpRoute httpRoute1 = new HttpRoute("{controller}");
            selfHostConfiguraiton.Routes.Add("Api2", httpRoute1);
            selfHostConfiguraiton.EnableCors(new EnableCorsAttribute("*", headers: "*", methods: "*"));
            _serv = new HttpSelfHostServer(selfHostConfiguraiton);
            _serv.OpenAsync();
        }
        public void Start()
        {
            //_log.Debug("123");
            //Console.WriteLine("456");
            StartSelfHost();
            _servJob.Start();
        }
        public void Stop() { _serv.CloseAsync(); _servJob.Stop(); }
    }
}
