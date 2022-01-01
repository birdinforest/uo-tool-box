using System;
using System.Drawing;
using Pastel;

namespace UOToolBox.Utility
{
    public class Logger
    {
        public enum Module
        {
            PipeClient, Parser
        }
        
        public static void Log(Module module, string message)
        {
            Console.WriteLine($"[{$"UTB \t\t| {Enum.GetName(module)}".Pastel(Color.DarkGreen)}] {message}");
        }
    }
}