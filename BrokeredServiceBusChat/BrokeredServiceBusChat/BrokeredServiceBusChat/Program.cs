using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokeredServiceBusChat
{
    class Program
    {
        static string ConnectionString = "";
        static string TopicPath = "chattopic";
        static void Main(string[] args)
        {
            Console.WriteLine("Enter name:");
            var username = Console.ReadLine();

            var manager = NamespaceManager.CreateFromConnectionString(ConnectionString);

            //Create Topic
            if (!manager.TopicExists(TopicPath))
            {
                manager.CreateTopic(TopicPath);
            }

            // Create subscription
            var description = new SubscriptionDescription(TopicPath, username)
            {
                AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
            };
            manager.CreateSubscription(description);

            // Create Clients
            var factory = MessagingFactory.CreateFromConnectionString(ConnectionString);
            var topicClient = factory.CreateTopicClient(TopicPath);
            var subscriptionClient = factory.CreateSubscriptionClient(TopicPath, username);

            // Create message pump
            subscriptionClient.OnMessage(msg => ProcessMessage(msg));

            var helloMessage = new BrokeredMessage("Has enetered the room...");
            helloMessage.Label = username;
            topicClient.Send(helloMessage);

            while(true)
            {
                var text = Console.ReadLine();
                if (text.Equals("exit")) break;

                var chatMessage = new BrokeredMessage(text);
                chatMessage.Label = username;
                topicClient.Send(chatMessage);
            }

            var byeMessage = new BrokeredMessage("Has left the room...");
            helloMessage.Label = username;
            topicClient.Send(byeMessage);

            factory.Close();

        }

        private static void ProcessMessage(BrokeredMessage msg)
        {
            var user = msg.Label;
            var body = msg.GetBody<string>();

            Console.WriteLine(user + " > " + body);
        }
    }
}

