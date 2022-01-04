#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Threading;
using UOToolBox.Utility;

namespace UOToolBox
{
    /// <summary>
    /// Track transmitted data package.
    /// It is necessary to know the start and end of a package, thus it could read the id and length.
    /// Large size package request reading many times, it tracks the step and execute OnFinish events once reading has been done.
    /// </summary>
    public struct ReadTracking
    {
        public delegate void OnFinishDelegates(ReadTracking instance);
        
        public int Id;
        public bool IsReading ;
        public int TotalLength;
        public int ReadLength;
        public byte[] Buffer;
        
        private OnFinishDelegates OnFinish;
        
        public ReadTracking(Action<ReadTracking> OnFinish)
        {
            Id = -1;
            IsReading = false;
            TotalLength = 0;
            ReadLength = 0;
            Buffer = new byte[] { };

            this.OnFinish = (ReadTracking instance) =>
            {
                instance.Id = -1;
                instance.IsReading = false;
                instance.TotalLength = 0;
                instance.ReadLength = 0;
            };
            this.OnFinish += new OnFinishDelegates(OnFinish);
            this.OnFinish += (ReadTracking instance) =>
            {
                instance.Buffer = Array.Empty<byte>();
            };
        }

        /// <summary>
        /// Initialize tracking information.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="totalLength"></param>
        public void Initialize(int id, int totalLength)
        {
            this.Id = id;
            this.TotalLength = totalLength;
            Buffer = new byte[totalLength];
        }

        /// <summary>
        /// Update tracking information.
        /// </summary>
        /// <param name="step"></param>
        public void Update(int step)
        {
            if (Id == -1 || TotalLength == 0) 
                return;
            
            ReadLength += step;

            if (ReadLength < TotalLength)
            {
                IsReading = true;
            }
            else
            {
                Id = -1;
                TotalLength = 0;
                ReadLength = 0;
                IsReading = false;
                OnFinish.Invoke(this);
            }
        }

        /// <summary>
        /// The length left.
        /// </summary>
        /// <returns></returns>
        public int LengthToRead()
        {
            return TotalLength - ReadLength;
        }
    }
    
    public class NamePipeClient
    {
        private static int numClients = 4;
        
        public delegate void OnPackageReceived(string text);

        public static OnPackageReceived OnRecv;
        
        // ID of transmitted data
        private static int _id = -1;

        public static ReadTracking Tracking = new (instance =>
        {
            // Log($"Read buffer. Length: {instance.Buffer.Length}. Text:" +
            //     $"\n{DebugPrintBuffer(instance.Buffer)}");

            if (PackageParser.IsJourney(ref instance.Buffer))
            {
                var text = PackageParser.ParseJourney(ref instance.Buffer);
                Log($"Read buffer. Length: {instance.Buffer.Length}. Text:\n{text}");
                if (OnRecv != null)
                {
                    OnRecv.Invoke(text);
                }
            }
        });
        
        public static void Start()
        {
            using var pipeClient =
                new NamedPipeClientStream(".", "pipe",
                    PipeDirection.InOut, PipeOptions.None,
                    TokenImpersonationLevel.Impersonation);

            Log("Connecting to server...\n");
            pipeClient.Connect();

            var streamBuffer = new StreamBuffer(pipeClient);

            // Validate the server's signature string.
            // var validation = streamBuffer.ReadString();
            // Log($"Validation str token: {validation}");
            // if (validation == "Hello!")
            // {
                // The client security token is sent with the first write.
                // Send the name of the file whose contents are returned
                // by the server.
                var filename =
                    "/Users/forrrest/ClassicUOLauncher-osx-x64-release/ClassicUO/Data/Client/JournalLogs/2021_12_23_23_44_21_journal.txt";
                streamBuffer.WriteString(filename);

                Log($"Send file name: {filename}\n");

                // // Print the file to the screen.
                // var respond = ss.ReadString();
                // Console.WriteLine($"[UOToolBox][PipeClient]Respond:\n{respond}\n");

                streamBuffer.WriteString("get_package");
                Log("Send message:\"get_package\"");

                while (true)
                {
                    streamBuffer.Read();
                }
            // }
            // else
            // {
            //     Log( "Server could not be verified.");
            // }

            // pipeClient.Close();
            // Give the client process some time to display results before exiting.
            // Thread.Sleep(2000);
        }

        private static string DebugPrintBuffer(byte[] data)
        {
            return Encoding.Default.GetString(data);
        }

        public static string PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }

            sb.Append("}");
            return sb.ToString();
        }

        public static void Log(string message)
        {
            Logger.Log(Logger.Module.PipeClient, message);
        }
        
        public static int GenerateDataId()
        {
            ++_id;
            
            // if (_id >= UInt16.MaxValue)
            //     _id = 0;
            
            if (_id >= 1024)
                _id = 0;

            return _id;
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

        public int ReadId()
        {
            int id;
            
            var value = ioStream.ReadByte();
            if (value == -1) return -1;
            id = value * 256;
            
            value = ioStream.ReadByte();
            if (value == -1) return -1;
            id += value;

            return id;
        }
        
        public int ReadLength()
        {
            int len;
            
            var value = ioStream.ReadByte();
            if (value == -1) return -1;
            len = value * 256;
            
            value = ioStream.ReadByte();
            if (value == -1) return -1;
            len += value;

            return len;
        }

        public void Read()
        {
            var isNewPackage = false;
            
            int id;
            if (NamePipeClient.Tracking.Id == -1)
            {
                id = ReadId();
                // No data
                if (id == -1)
                    return;

                isNewPackage = true;
            }
            else
            {
                id = NamePipeClient.Tracking.Id;
            }

            var len = isNewPackage ? ReadLength() : NamePipeClient.Tracking.LengthToRead();
            // Unexpected. It only has id but has no data following the id.
            if (len == -1)
                return;

            if (isNewPackage)
            {
                NamePipeClient.Tracking.Initialize(id, len);
            }
            
            
            // var step = ioStream.Read(inBuffer, 0, len);
            var step = ioStream.Read(NamePipeClient.Tracking.Buffer, NamePipeClient.Tracking.ReadLength, len);
            
            NamePipeClient.Log($"|Read\t| id: {id}, len: {len}, step: {step}");
            
            NamePipeClient.Tracking.Update(step);
        }

        // public string ReadString()
        // {
        //     var inBuffer = Read();
        //
        //     var text = streamEncoding.GetString(inBuffer);
        //     NamePipeClient.Log("DEBUG >> " + text);
        //     
        //     return inBuffer.Length == 0 
        //         ? "" 
        //         : streamEncoding.GetString(inBuffer);
        // }

        public int Write(byte[] outBuffer)
        {
            var id = NamePipeClient.GenerateDataId();
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int) UInt16.MaxValue;
            }
            
            NamePipeClient.Log($"|Write\t| id: {id}, len: {len}");
            
            ioStream.WriteByte((byte) (id / 256));
            ioStream.WriteByte((byte) (id & 255));

            ioStream.WriteByte((byte) (len / 256));
            ioStream.WriteByte((byte) (len & 255));
            
            ioStream.Write(outBuffer, 0, len);
            
            ioStream.Flush();

            return outBuffer.Length + 4;
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            return Write(outBuffer);
        }
    }
}