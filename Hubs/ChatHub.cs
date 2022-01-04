using System;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.SignalR;
using Timer = System.Timers.Timer;
using UOToolBox.Interface;
using UOToolBox.Models;

namespace UOToolBox.Hubs
{
    public class Prop
    {
        public static bool doBroadcast = false;
        public static Timer aTimer;
    }
    public class ChatHub: Hub<IChatClient>
    {
        
        public async Task SendMessage(ChatMessage message)
        {
            Console.WriteLine($"ChatHub send message. User:{message.User}, Message:{message.Message}");
            await Clients.All.ReceiveMessage(message);
        }

        public async Task StartBroadcastMessage()
        {
            Console.WriteLine($"ChatHub start broadcast");

            // await Clients.All.ReceiveMessage(new ChatMessage
            // {
            //     User = "Server",
            //     Message = $"This is broadcast."
            // });

            Prop.doBroadcast = true;
            
            // Create a timer and set a two second interval.
            Prop.aTimer = new System.Timers.Timer();
            Prop.aTimer.Interval = 2000;
            
            // Hook up the Elapsed event for the timer
            var onTimedEvent = new ElapsedEventHandler(async (Object source, System.Timers.ElapsedEventArgs e) =>
            {
                if (!Prop.doBroadcast)
                    return;
                
                var chatMessage = new ChatMessage
                {
                    User = "Server",
                    Message = $"This is broadcast at {e.SignalTime}."
                };
                
                Console.WriteLine($"ChatHub send message. User:{chatMessage.User}, Message:{chatMessage.Message}");
                await Clients.All.ReceiveMessage(chatMessage);
            });
            
            Prop.aTimer.Elapsed += onTimedEvent as ElapsedEventHandler;
            
            // Have the timer fire repeated events (true is the default)
            Prop.aTimer.AutoReset = true;
            
            // Start the timer
            Prop.aTimer.Enabled = true;
        }

        public async Task StopBroadcastMessage()
        {
            Console.WriteLine($"ChatHub stop broadcast");
            Prop.doBroadcast = false;
        }
    }
}