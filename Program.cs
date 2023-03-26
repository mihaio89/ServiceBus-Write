using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace ServiceBus;

    class Program
    {

        static string connectionString;
        static string queueName;

        static string file;
        static async Task Main(string[] args)
        {
           // string currentDateTime= DateTime.Now.ToString("yyyyMMddHHmmssfff");
          //  Console.WriteLine(currentDateTime);
          //  var stopwatch = Stopwatch.StartNew();

         if (args.Length != 0) { file = args[0]; };
            
            // Load the configuration file
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
             //   .AddJsonFile("appsettings.dev.json")
                .AddJsonFile("appsettings.int.json")
                .Build();

            // Get the configuration values
            connectionString = configuration["ServiceBus:ServiceBusConnectionString"];
            queueName = configuration["ServiceBus:QueueName"];

        //  await Test(args);
            await ForDispatch();

        }

        static async Task Test(string[] args) 
        {

            string gk_suffix = "";
            var input = "test.json";
            var messageJson = await File.ReadAllTextAsync(input);

            
            int total = int.Parse(args[0]);

            for (int i = 0; i < total; i++)
            {
                Thread.Sleep(100);

                string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                Random random = new Random();
                if (args[1] == "r") {
                    gk_suffix = random.Next(2) == 0 ? "12" : "34";
                } else {
                    gk_suffix = args[1];
                }


                Guid guid = Guid.NewGuid();
                string guidString = guid.ToString("N") + "-" + gk_suffix;

                string updatedMessageJson = messageJson
                        .Replace("VALUE-i_ext", currentDateTime)
                        .Replace("VALUE-GK", guidString);

            //   Console.WriteLine(updatedMessageJson);

                // Parse the message JSON into a JObject
              //  var messageObject = JObject.Parse(updatedMessageJson);

                // Get the session ID from the "ID" attribute of the message
                var sessionId = gk_suffix; //(string)messageObject["it_bit_it"][0]["ID"];

                // Create a ServiceBusClient and a ServiceBusSender
                await using var client = new ServiceBusClient(connectionString);
                await using var sender = client.CreateSender(queueName);

                // Create a ServiceBusMessage with the message body and session ID
                var messageBody = updatedMessageJson;
                var message = new ServiceBusMessage(messageBody)
                {
                    SessionId = sessionId
                };

                // Send the message and print a confirmation message
                await sender.SendMessageAsync(message);

                Console.WriteLine($"sent message SessionId: '{sessionId}' : i_ext = '{currentDateTime}', GK = '{guidString}'");

            }

          //  stopwatch.Stop();
           // Console.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms");
        }

        static async Task ForDispatch() 
        {
            const string gk_suffix = "11";
            string currentDateTime = DateTime.Now.ToString("yyyyMMddHHmmsss");

            string input;
        
            switch (file)
            {
                case "m":
                    input = "collector_bp_mixed.json";
                    break;
                case "n":
                    input = "collector_bp_not_exist.json";
                    break;
                case "2":
                     input = "collector_2bp_exist.json";
                    break;
                default:
                    input = "collector.json";
                    break;
            }

            var messageJson = await File.ReadAllTextAsync(input);


                Guid guid = Guid.NewGuid();
                string guidString = guid.ToString("N") + "-" + gk_suffix;

                string updatedMessageJson = messageJson
                        .Replace("ZYYYYMMDD", currentDateTime);
                      //  .Replace("VALUE-GK", guidString);


                var sessionId = gk_suffix; 

                // Create a ServiceBusClient and a ServiceBusSender
                await using var client = new ServiceBusClient(connectionString);
                await using var sender = client.CreateSender(queueName);

                // Create a ServiceBusMessage with the message body and session ID
                var messageBody = updatedMessageJson;
                var message = new ServiceBusMessage(messageBody)
                {
                    SessionId = sessionId
                };

                // Send the message and print a confirmation message
                await sender.SendMessageAsync(message);

                Console.WriteLine($"sent message SessionId: '{sessionId}' : srctaid = '{currentDateTime}'");//, GK = '{guidString}'");

        }
    }