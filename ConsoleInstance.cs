using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleWorker
{
    public class ConsoleInstance
    {
        public static WorkerStatus AddModule(string module64)
        {
            try
            {
                using NamedPipeClientStream pipeClient = new(".", "mconnect", PipeDirection.Out);
                StreamString streamString = new(pipeClient);
                Span<Byte> buffer = new(new byte[module64.Length]);
                if (Convert.TryFromBase64String(module64, buffer, out int con))
                {
                    pipeClient.Connect();
                    streamString.WriteString("addmodule " + module64);
                    pipeClient.Close();
                    return WorkerStatus.Success;
                }
                else
                {
                    return WorkerStatus.WrongCode;
                }
            }
            catch
            {
                return WorkerStatus.FatalError;
            }
        }
        public static WorkerStatus ExecuteCommand(string commandText)
        {
            try
            {
                using NamedPipeClientStream pipeClient = new(".", "mconnect", PipeDirection.Out);
                StreamString streamString = new(pipeClient);
                pipeClient.Connect();
                streamString.WriteString("executecommand " + commandText);
                pipeClient.Close();
                return WorkerStatus.Success;
            } catch
            {
                return WorkerStatus.FatalError;
            }
        }
        public enum WorkerStatus
        {
            FatalError,
            Success,
            WrongCode
        }
    }
    public class StreamString
    {
        private readonly Stream ioStream;
        private readonly UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}
