using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                int z = 5;//iterations to seed the queue of work
                for (int i = 1; i <= z; i++)
                {
                    Task t1 = factory.StartNew(() =>
                    {

                        //create customer through odata rest call
                        Console.WriteLine($"starting with iteration {Task.CurrentId.ToString()}");
                        FunctionAppTest t = new FunctionAppTest();
                        Console.WriteLine($"iteration {Task.CurrentId.ToString()} says: {t.createCustomerREST()}");
                        Console.WriteLine($"ending with iteration {Task.CurrentId.ToString()}");

                        //do some other integration test....

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

    public class FunctionAppTest
    {

        public static AuthenticationResult getOauthHeaderNTESandbox()
        {

            AuthenticationContext authenticationContext =
            new AuthenticationContext("https://login.microsoftonline.com/avanademfg.onmicrosoft.com", false);
            AuthenticationResult authenticationResult;
            authenticationResult =
                authenticationContext.AcquireTokenAsync("https://avadevvm02a568e73aab0f182fdevaos.cloudax.dynamics.com",
                    new ClientCredential("app clientId", "app secret")).Result;
            return authenticationResult;
        }

        public string createCustomerREST()
        {
            string ret;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://avadevvm02a568e73aab0f182fdevaos.cloudax.dynamics.com/");
            client.DefaultRequestHeaders.Add("Authorization", FunctionAppTest.getOauthHeaderNTESandbox().CreateAuthorizationHeader());
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "data/CustomersV3");
            var parametersToPass = new
            {
                CustomerGroupId = "10",
                PartyType = "Person",
                PersonFirstName = "AVA",
                PersonLastName = "Demo Test",
                PrimaryContactEmail = "blah@blah.com",
                SalesCurrencyCode = "USD",
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(parametersToPass), Encoding.UTF8, "application/json");
            var result = client.SendAsync(request).Result;
            if (result.IsSuccessStatusCode)
            {
                ret = result.Content.ReadAsStringAsync().Result;
            }
            else
            {
                ret = result.Content.ReadAsStringAsync().Result;
            }
            client.Dispose();
            return ret;
        }
    }
}
