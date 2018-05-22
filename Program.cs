using System;
using System.Configuration;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace PaaSDevL200Redis3
{
    class Program
    {
        static void Main(string[] args)
        {
            int workerThreads, IOThreads, processedRequests = 0;
            int requestCount = 300;

            // Generating value 
            int valueSize = 1000000;
            var rand = new Random();
            byte[] file = new byte[valueSize];
            rand.NextBytes(file);

            string connectionString = ConfigurationManager.ConnectionStrings["RedisConnectionString"].ConnectionString;
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(connectionString);
            IDatabase db = connection.GetDatabase();

            // Threadpool part
            //System.Threading.ThreadPool.SetMinThreads(1, 1);
            System.Threading.ThreadPool.GetMinThreads(out workerThreads, out IOThreads);
            Console.WriteLine("MinThreadPool values used : {0} Worker and {1} IO", workerThreads, IOThreads);
            Console.WriteLine("Trying to run {0} sets\nPress a key to start", requestCount);
            Console.ReadLine();
            try
            {
                Parallel.For(0, requestCount,
                (i) =>
                {
                    processedRequests++;
                    db.StringSet((i % 240).ToString(), value: file);
                    Console.WriteLine("Key {0} : Set", (i % 240));
                });
                Console.WriteLine("- All {0} sets finished without errors (Value size is {1} KB) -\n Press a button to exit", requestCount, (valueSize/1000));
                Console.ReadLine();
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("- Exceptions occurred -\n");
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    Console.WriteLine(e.ToString() + "\n");
                }
                Console.WriteLine("\n- {0}/{1} faced an exception -\n Press a button to exit", ae.InnerExceptions.Count, processedRequests);
                Console.ReadLine();
            }

        }
    }
}
