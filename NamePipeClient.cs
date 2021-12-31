#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace UOToolBox
{
    public class NamePipeClient
    {
        private static int numClients = 4;
        public static int total = 0;

        public static void Start()
        {
            var pipeClient =
                new NamedPipeClientStream(".", "pipe",
                    PipeDirection.InOut, PipeOptions.None,
                    TokenImpersonationLevel.Impersonation);

            Console.WriteLine("[UOToolBox][PipeClient]Connecting to server...\n");
            pipeClient.Connect();

            var ss = new StreamString(pipeClient);

            // Validate the server's signature string.
            var validation = ss.ReadString();
            Console.WriteLine($"[UOToolBox][PipeClient]Validation str token: {validation}\n");
            if (validation == "I am the one true server!")
            {
                // The client security token is sent with the first write.
                // Send the name of the file whose contents are returned
                // by the server.
                var filename =
                    "/Users/forrrest/ClassicUOLauncher-osx-x64-release/ClassicUO/Data/Client/JournalLogs/2021_12_23_23_44_21_journal.txt";
                ss.WriteString(filename);

                Console.WriteLine($"[UOToolBox][PipeClient]Send file name: {filename}\n");

                // // Print the file to the screen.
                // var respond = ss.ReadString();
                // Console.WriteLine($"[UOToolBox][PipeClient]Respond:\n{respond}\n");

                ss.WriteString("get_package");
                Console.WriteLine($"[UOToolBox][PipeClient]Send message:\"get_package\"");

                var streamBuffer = new StreamBuffer(pipeClient);

                var package = streamBuffer.Read();
                Console.WriteLine($"[UOToolBox][PipeClient] 001 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");

                package = streamBuffer.Read();
                Console.WriteLine($"[UOToolBox][PipeClient] 002 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");

                package = streamBuffer.Read();
                Console.WriteLine($"[UOToolBox][PipeClient] 003 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");

                package = streamBuffer.Read();
                Console.WriteLine($"[UOToolBox][PipeClient] 004 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");

                package = streamBuffer.Read();
                Console.WriteLine($"[UOToolBox][PipeClient] 005 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");

                package = streamBuffer.Read();
                Console.WriteLine($"[UOToolBox][PipeClient] 006 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");

                Console.WriteLine($"[UOToolBox][PipeClient] 007 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");

                package = streamBuffer.Read();
                Console.WriteLine($"[UOToolBox][PipeClient] 008 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");

                package = streamBuffer.Read();
                Console.WriteLine($"[UOToolBox][PipeClient] 009 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");

                package = streamBuffer.Read();
                Console.WriteLine($"[UOToolBox][PipeClient] 010 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");

                package = streamBuffer.Read();
                Console.WriteLine($"[UOToolBox][PipeClient] 011 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");

                package = streamBuffer.Read();
                Console.WriteLine($"[UOToolBox][PipeClient] 012 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");

                package = streamBuffer.Read();
                Console.WriteLine($"[UOToolBox][PipeClient] 013 Read buffer. Length: {package.Length}. Text:" +
                                  $"\n{DebugPrintBuffer(package)}");
            }
            else
            {
                Console.WriteLine("[UOToolBox][PipeClient]Server could not be verified.");
            }

            // pipeClient.Close();
            // Give the client process some time to display results before exiting.
            Thread.Sleep(2000);
        }

        private static string DebugPrintBuffer(byte[] data)
        {
            return Encoding.Default.GetString(data);
        }

        private static string PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }

            sb.Append("}");
            return sb.ToString();
        }
    }

    // Defines the data protocol for reading and writing strings on our stream.
    public class StreamString
    {
        private NamedPipeClientStream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(NamedPipeClientStream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            var sb = new StringBuilder("");

            // var result = 1;
            // while (result != 0)
            // {
            //     
            // }
            int len;

            var tempByte = ioStream.ReadByte();
            len = tempByte * 256; // ioStream.ReadByte() * 256;

            tempByte = ioStream.ReadByte();
            len += tempByte; // ioStream.ReadByte();

            Console.WriteLine("\n[UOToolBox][PipeClient]Read length: " + len);

            var inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int) UInt16.MaxValue;
            }

            ioStream.WriteByte((byte) (len / 256));
            ioStream.WriteByte((byte) (len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            Console.WriteLine("\n[UOToolBox][PipeClient]Write length: " + len);

            return outBuffer.Length + 2;
        }
    }

    // Defines the data protocol for reading and writing strings on our stream.
    public class StreamBuffer
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamBuffer(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public byte[] Read()
        {
            int len;
            try
            {
                var result = ioStream.ReadByte();
                Console.WriteLine($"[UOToolBox][PipeClient] StreamBuffer.ReadByte 01 result: {result}");
                if (result == -1) return new byte[] { };

                len = result * 256;

                result = ioStream.ReadByte();
                Console.WriteLine($"[UOToolBox][PipeClient] StreamBuffer.ReadByte 02 result: {result}");
                if (result == -1) return new byte[] { };

                len += result;

                Console.WriteLine($"[UOToolBox][PipeClient] StreamBuffer.Read Length: {len}");

                // var inBuffer = new byte[len];
                var inBuffer = new byte[65535];

                // var step = ioStream.Read(inBuffer, 0, len);
                var step = ioStream.Read(inBuffer, 0, 65535);

                Console.WriteLine($"[UOToolBox][PipeClient] StreamBuffer. Reading step 01: " + step);

                // if (step == len)
                // {
                //     var byteStep = ioStream.ReadByte();
                //     Console.WriteLine($"[UOToolBox][PipeClient] One more byte step: " + byteStep);
                // }

                // int stepLength = 0;
                // while (true)
                // {
                //     var step = ioStream.Read(inBuffer, stepLength - 1, len - stepLength);
                //     if (step == 0) break;
                //     Console.WriteLine($"[UOToolBox][PipeClient] StreamBuffer stepLength : {len}");
                //     stepLength += step;
                // }

                NamePipeClient.total += step;
                Console.WriteLine($"[UOToolBox][PipeClient] StreamBuffer. Total done step: " + NamePipeClient.total);

                return inBuffer;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: BufferStream error message: {e.Message}");
                return new byte[] { };
            }
        }

        public int Write(byte[] outBuffer)
        {
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int) UInt16.MaxValue;
            }

            ioStream.WriteByte((byte) (len / 256));
            ioStream.WriteByte((byte) (len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}