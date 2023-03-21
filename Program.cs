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
        static async Task Main(string[] args)
        {
            // Load the configuration file
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.int.json")
                .Build();

            // Get the configuration values
            var connectionString = configuration["ServiceBus:ServiceBusConnectionString"];
            Console.WriteLine($"Connection string: {connectionString}");

            var topicName = configuration["ServiceBus:TopicName"];
            Console.WriteLine($"Topic name: {topicName}");

            // Read the message JSON from file
            var messageJson = await File.ReadAllTextAsync("message.json");

            for (int i = 0; i < 1; i++)
            {
                // Sleep for a short time to avoid sending all messages at once
                await Task.Delay(100);

                string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string gk_suffix = "01";

                Guid guid = Guid.NewGuid();
                string guidString = guid.ToString("N") + "-" + gk_suffix;

                string updatedMessageJson = messageJson
                    .Replace("VALUE-i_ext", currentDateTime)
                    .Replace("VALUE-GK", guidString);

                // Parse the message JSON into a JObject
                var messageObject = JObject.Parse(updatedMessageJson);

                // Get the session ID from the "ID" attribute of the message
                var sessionId = gk_suffix;

                // Create a ServiceBusClient and a ServiceBusSender
                await using var client = new ServiceBusClient(connectionString);
                await using var sender = client.CreateSender(topicName);

                // Create a ServiceBusMessage with the message body and session ID
                var messageBody = updatedMessageJson;
                var message = new ServiceBusMessage(messageBody)
                {
                    SessionId = sessionId
                };

                // Send the message and print a confirmation message
                await sender.SendMessageAsync(message);

                Console.WriteLine($"Sent message with SessionId: '{sessionId}' i_ext = '{currentDateTime}', GK = '{guidString}'");
            }
        }
    }
}
