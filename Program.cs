using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace ServiceBus
{
    class Program
    {
        static string gk_suffix = "";
        static async Task Main(string[] args)
        {
           // string currentDateTime= DateTime.Now.ToString("yyyyMMddHHmmssfff");
          //  Console.WriteLine(currentDateTime);
          //  var stopwatch = Stopwatch.StartNew();
            
            // Load the configuration file
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
             //   .AddJsonFile("appsettings.dev.json")
                .AddJsonFile("appsettings.int.json")
                .Build();

            // Get the configuration values
            var connectionString = configuration["ServiceBus:ServiceBusConnectionString"];
            var queueName = configuration["ServiceBus:QueueName"];


            // Read the message JSON from file
            var messageJson = await File.ReadAllTextAsync("message.json");

            
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
                var messageObject = JObject.Parse(updatedMessageJson);

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
    }
}