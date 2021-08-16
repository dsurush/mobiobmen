using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using MobiObmen.Services;
using System.Data.Entity;
using MobiObmen.Context;
using MobiObmen.Migrations;
using log4net;
using Newtonsoft.Json;

namespace MobiObmen
{
    class Program
    {

        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));
        public static void Automigrate()
        {
            try
            {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<ObmenDataContext, Configuration>());
            }
            catch (Exception e)
            {
                _log.Error("Error with migration db", e);
            }
        }
        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
            Automigrate();
            Host h = HostFactory.New(x =>
            {
                x.Service<BootService>(s =>
                {
                    s.ConstructUsing(name => new BootService());
                    s.WhenStarted(wd => wd.Start());
                    s.WhenStopped(wd => wd.Stop());
                });
                x.RunAsLocalSystem();
                x.SetDescription("Совершения транзакций");
                x.SetDisplayName("BM.MOBIOBMEN.Background");
                x.SetServiceName("BM.MOBIOBMEN.Background");
            });
            h.Run();
        }
    }
}
