using System;
using System.Buffers.Binary;
using System.Text;
using UOToolBox.Utility;

namespace UOToolBox.Utility
{
    public class PackageParser
    {
        public static bool IsJourney(ref byte[] buffer)
        {
            // BinaryPrimitives.TryReadUInt32BigEndian(buffer, out uint v);
            var id = buffer[0];
            Logger.Log(Logger.Module.Parser, $" id:{id}");
            return id == 174;
        }
        
        public static string ParseJourney(ref byte[] buffer)
        {
            ReadOnlySpan<byte> data = buffer;
            var name = Encoding.Default.GetString(data.Slice(18, 30));
            var content = Encoding.Default.GetString(data.Slice(48));

            return $"{name}:{content}";
        }
    }
}