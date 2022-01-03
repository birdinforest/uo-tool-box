using System;
using System.Drawing;
using Pastel;

namespace UOToolBox.Utility
{
    public class Logger
    {
        // Implement dynamic change by config.
        public static int ModuleFilter = (int)(Module.LocalFileSystem | Module.SignalR);
        
        [Flags]
        public enum Module
        {
            PipeClient = 1 << 1, 
            Parser = 1 << 2, 
            LocalFileSystem = 1 << 3, 
            SignalR = 1 << 4,
        }
        
        public static void Log(Module module, string message)
        {
            var value = ModuleFilter & (int) module;
            if (value == (int) module)
            {
                Console.WriteLine($"[{$"UTB-Server\t| {Enum.GetName(module)}".Pastel(Color.DarkGreen)}] {message}");
            }
        }
    }
}