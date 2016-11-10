using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rabbitmq_demo_service
{
    public class Program
    {
        public static void Main()
        {
            var services = new Services();
            services.Received += Console.WriteLine;

            Console.ReadKey();
        }
    }
}
