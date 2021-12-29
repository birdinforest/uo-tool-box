using System;
using System.Runtime.InteropServices;
using System.Threading;
using CUO_API;

namespace Assistant
{
    public class Engine
    {
        private static unsafe PluginHeader* header;
        
        public static unsafe void Install(PluginHeader* plugin)
        {
            header = plugin;
            
            var app = new App();
            app.Install(header);
            
            var thread = new Thread(KeepingAlive);
            thread.Start();

            Console.WriteLine($"[Plugin] start thread {thread.Name}");
        }
        
        private static unsafe void KeepingAlive()
        {
            // var app = new App();
            // app.Install(header);

            // while (true)
            // {
            //     if (Console.ReadLine() != "exit") 
            //         continue; 
            // }
        }
        
        public int Add(int a, int b)
        {
            return a + b;
        }

        public int Sub(int a, int b)
        {
            return a - b;
        }

        public string Hello(string name)
        {
            return "Hello " + name;
        }

        public static void Main()
        {
            Console.WriteLine("Hello world.");
        }
    }

    public class App
    {
        private static OnPacketSendRecv _sendToClient, _sendToServer, _recv, _send;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate string dOnGetUOFilePath();

        public unsafe void Install(PluginHeader* plugin)
        {
            var _uoFilePath =
                (OnGetUOFilePath) Marshal.GetDelegateForFunctionPointer(plugin->GetUOFilePath, typeof(OnGetUOFilePath));

            Console.WriteLine("[Plugin] clientPath: " + _uoFilePath());

            Console.WriteLine("[Plugin] Hello from dynamic dll.");

            _recv = OnRecv;
            plugin->OnRecv = Marshal.GetFunctionPointerForDelegate(_recv);
        }

        private unsafe bool OnRecv(ref byte[] data, ref int length)
        {
            Console.WriteLine($"[Plugin] OnRecv. Length:{length}, data:{data}");
            return true;
        }
    }
}