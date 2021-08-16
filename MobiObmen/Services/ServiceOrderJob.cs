using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiObmen.Services
{
    class ServiceOrderJob
    {
        public void Start()
        {
            Console.WriteLine("Server is started");
        }
        //При закрытие приложения/////////////////////////////////////////////////////////////////////////////////////
        public void Stop()
        {
            Console.WriteLine("Stop Program");
        }
    }
}
