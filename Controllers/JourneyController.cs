using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using UOToolBox.Hubs;
using UOToolBox.Interface;
using UOToolBox.Models;
using UOToolBox.Utility;

namespace UOToolBox.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JourneyController : ControllerBase
    {
        private static bool doBroadcast = false;
        private static System.Timers.Timer aTimer;

        private readonly IHubContext<ChatHub, IChatClient> _chatHub;

        private readonly ILogger<JourneyController> _logger;

        public JourneyController(ILogger<JourneyController> logger, IHubContext<ChatHub, IChatClient> chatHub)
        {
            _logger = logger;
            _chatHub = chatHub;
            NamePipeClient.OnRecv = PushToClient;
        }

        /// <summary>
        /// Get journey files at local directory.
        /// API: [url]/journey
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Journey> Get()
        {
            var dir = "/Users/forrrest/projects/ClassicUO/bin/Debug/Data/Client/JournalLogs";
            if (System.IO.Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir);
                var sb = new StringBuilder();
                foreach (var file in files)
                {
                    var text = ReadFile(file);
                    sb.Append(text);
                    Logger.Log(Logger.Module.LocalFileSystem, $"Reading file: {file}");
                }

                return new[] {new Journey(sb.ToString())};
            }
            else
            {
                Logger.Log(Logger.Module.LocalFileSystem, $"Directory {dir} is not exist.");
            }

            return Array.Empty<Journey>();
        }

        /// <summary>
        /// API: [url]/journey/message.
        /// Post.
        /// </summary>
        /// <returns></returns>
        [HttpPost("message")]
        public async Task Post(ChatMessage message)
        {
            // run some logic...

            await _chatHub.Clients.All.ReceiveMessage(message);
        }

        /// <summary>
        /// Start to push runtime journey to client.
        /// </summary>
        [HttpPost("loop-start")]
        public async Task StartBroadcastLoop()
        {
            // if (doBroadcast)
            //     return;
            //
            // // run some logic...
            //
            // doBroadcast = true;
            //
            // // Create a timer and set a two second interval.
            // aTimer = new System.Timers.Timer();
            // aTimer.Interval = 2000;
            //
            // // Hook up the Elapsed event for the timer
            // var onTimedEvent = new System.Timers.ElapsedEventHandler(
            //     async (Object source, System.Timers.ElapsedEventArgs e) =>
            //     {
            //         if (!doBroadcast)
            //             return;
            //
            //         var chatMessage = new ChatMessage
            //         {
            //             User = "Server",
            //             Message = $"This is broadcast at {e.SignalTime}."
            //         };
            //
            //         Console.WriteLine(
            //             $"BroadcastController send message. User:{chatMessage.User}, Message:{chatMessage.Message}");
            //         await _chatHub.Clients.All.ReceiveMessage(chatMessage);
            //     });
            //
            // aTimer.Elapsed += onTimedEvent;
            //
            // // Have the timer fire repeated events (true is the default)
            // aTimer.AutoReset = true;
            //
            // // Start the timer
            // aTimer.Enabled = true;

            // NamePipeClient.OnRecv += PushToClient;
        }

        async void PushToClient(string text)
        {
            var message = new ChatMessage
            {
                User = "",
                Message = text,
            };

            Logger.Log(Logger.Module.SignalR, $"{message.User} | {message.Message}");
                
            await _chatHub.Clients.All.ReceiveMessage(message);
        }

        [HttpPost("loop-stop")]
        public async Task StopBroadcastLoop()
        {
            // NamePipeClient.OnRecv -= PushToClient;
        }

        private string ReadFile(string path)
        {
            try
            {
                using var sr = new StreamReader(path);
                return sr.ReadToEnd();
            }
            catch (IOException e)
            {
                return "The file could not be read:\n" + e.Message;
            }
        }
    }
}