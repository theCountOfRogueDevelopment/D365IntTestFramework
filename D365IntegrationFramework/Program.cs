using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace D365IntegrationFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Task> tasks = new List<Task>();
            LimitedConcurrencyLevelTaskScheduler queue = new LimitedConcurrencyLevelTaskScheduler(4);//worker threads
            TaskFactory factory = new TaskFactory(queue);
            try
            {

                if (System.Net.ServicePointManager.SecurityProtocol == (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls))
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                Console.WriteLine("started " + DateTime.Now.ToLongTimeString());
                var watch = System.Diagnostics.Stopwatch.StartNew();
                int z = 5;//iterations
                for (int i = 1; i <= z; i++)
                {
                    Task t1 = factory.StartNew(() =>
                    {

                        //create customer through odata rest
                        Console.WriteLine($"starting with iteration {Task.CurrentId.ToString()}");
                        Console.WriteLine($"ending with iteration {Task.CurrentId.ToString()}");

                    }, CancellationToken.None, TaskCreationOptions.HideScheduler, queue);
                    tasks.Add(t1);
                }


                Task.WaitAll(tasks.ToArray());
                watch.Stop();

                Console.WriteLine($"{z} method calls in seconds = " + watch.Elapsed.TotalSeconds.ToString());
                Console.WriteLine($"{z} method calls in minutes = " + watch.Elapsed.TotalMinutes.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.Message);
            }


            Console.WriteLine("done!");
            Console.ReadLine();
        }
    }
}
