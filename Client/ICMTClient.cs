using ICMT.Core.Helpers;
using ICMT.Core.Messages;
using System.Net.NetworkInformation;

namespace ICMT
{
    internal class ICMTClient
    {
        public int Timeout { get; set; } = 10000;
        /// <summary>
        /// If a DataMessage fails to get a success response, how many times to retry sending it before continuing?
        /// </summary>
        public int MaxDataMessageRetries { get; set; } = 5;
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
            var setupStatus = SendSetupMessage(file);
            if(setupStatus != IPStatus.Success)
            {
                throw new Exception("Failed to setup-ping target. Ping status returned as: " + setupStatus);
            }
            SendFile(file);
            var checksum = ChecksumHelper.GetFileChecksum(file);
            var completionMessage = SendCompletionMessage(checksum);
            if (completionMessage != IPStatus.Success)
            {
                throw new Exception("Failed to complete-ping target. Ping status returned as: " + completionMessage);
            }
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


        private void SendFile(string file)
        {
            using (var br = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                byte[] buff = new byte[1400];
                int r = 0;
                while (0 < (r = br.Read(buff)))
                {
                    SendDataMessage(buff, (ushort)r);
                }
            }
        }

        private void SendDataMessage(byte[] chunk, ushort dataLength)
        {
            var data = DataMessage.Empty();
            data.SequenceNumber = sequenceNum++;
            data.SessionId = sessionId;
            data.Data = chunk;
            data.DataLength = dataLength;

            _sendDataMessage(data, 1);

        }

        private void _sendDataMessage(DataMessage msg, int tryCount)
        {
            var buffer = msg.Serialize();

            var reply = pinger.Send(Host, Timeout, buffer, pingOptions);
            if (reply.Status != IPStatus.Success && tryCount < MaxDataMessageRetries)
            {
                _sendDataMessage(msg, ++tryCount);
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
