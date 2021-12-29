using System;

namespace DefaultNamespace
{
    public class DataTransfer
    {
        public unsafe bool OnRecv(ref byte[] data, ref int length)
        {
            Console.WriteLine($"[Plugin] OnRecv. Length:{length}, data:{data}");
            return true;
        } 
    }
}