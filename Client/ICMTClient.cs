using ICMT.Core.Helpers;
using IMCT.Core.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace IMCT
{
    internal class ICMTClient
    {
        public int Timeout { get; set; } = 10000;
        public string Host { get; private set; }

        private Ping pinger;
        private PingOptions pingOptions;

        private uint sequenceNum = 0;
        private byte[] sessionId;

        public ICMTClient(string host)
        {
            sequenceNum = 0;
            sessionId = new byte[4];
            new Random().NextBytes(sessionId);

            pinger = new Ping();
            pingOptions = new PingOptions();
            pingOptions.DontFragment = true;

            Host = host;
        }

        public void Send(string file)
        {
            SendSetupMessage(file);
            SendDataMessage(file);
            var checksum = ChecksumHelper.GetFileChecksum(file);
            SendCompletionMessage(checksum);
        }

        private IPStatus SendSetupMessage(string file)
        {
            var fileName = Path.GetFileName(file);

            var setup = SetupMessage.Empty();

            setup.SequenceNumber = sequenceNum++;
            setup.SessionId = sessionId;
            setup.FileName = fileName;
            setup.FileNameLength = (byte)setup.FileName.Length;

            var buffer = setup.Serialize();

            PingReply reply = pinger.Send(Host, Timeout, buffer, pingOptions);
            return reply.Status;
        }

        private void SendDataMessage(string file)
        {
            using (var br = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                byte[] buff = new byte[1400];
                int r = 0;
                while (0 < (r = br.Read(buff)))
                {
                    var data = DataMessage.Empty();
                    data.SequenceNumber = sequenceNum++;
                    data.SessionId = sessionId;
                    data.Data = buff;
                    data.DataLength = (UInt16)r;

                    var buffer = data.Serialize();

                    _ = pinger.Send(Host, Timeout, buffer, pingOptions);
                }
            }
        }

        private IPStatus SendCompletionMessage(byte[] checksum)
        {
            var compl = CompletionMessage.Empty();
            compl.SequenceNumber = sequenceNum++;
            compl.SessionId = sessionId;
            compl.Checksum = checksum;

            var buffer = compl.Serialize();

            var reply = pinger.Send(Host, Timeout, buffer, pingOptions);
            return reply.Status;
        }
    }
}
