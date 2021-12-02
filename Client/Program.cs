using Core.Messages;
using System.Net.NetworkInformation;
using System.Text;

namespace ICMPTransfer
{
    class Client
    {
        public static void Main()
        {
            Console.WriteLine("Hello, World!");

            var photo = @"photo.jpg";

            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            options.DontFragment = true;
            int timeout = 10000;

            UInt16 seq = 0;
            byte[] sessionId = new byte[4];
            new Random().NextBytes(sessionId);

            var setup = SetupMessage.Empty();

            setup.SequenceNumber = seq++;
            setup.SessionId = sessionId;
            setup.FileName = "photo.jpg";
            setup.FileNameLength = (byte)setup.FileName.Length;

            var buffer = setup.Serialize();

            PingReply reply = pingSender.Send("hurtig.ninja", timeout, buffer, options);

            Console.WriteLine("Status: {0}", reply.Status);
            Console.WriteLine("Address: {0}", reply.Address.ToString());
            Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
            Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
            Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
            Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);

            Console.WriteLine("----------------------");

            Console.ReadLine();

            using (var br = new BinaryReader(new FileStream(photo, FileMode.Open)))
            {
                int totalSent = 0;
                byte[] buff = new byte[1400];
                int r = 0;
                while (0 < (r = br.Read(buff)))
                {
                    var data = DataMessage.Empty();
                    data.SequenceNumber = seq++;
                    data.SessionId = sessionId;
                    data.Data = buff;
                    data.DataLength = (UInt16)r;

                    buffer = data.Serialize();

                    reply = pingSender.Send("hurtig.ninja", timeout, buffer, options);

                    totalSent += r;
                    Console.WriteLine($"{totalSent} bytes sent..");

                    //Console.ReadLine();

                }
            }



            Console.WriteLine("----------------------");

            Console.ReadLine();


            var compl = CompletionMessage.Empty();
            compl.SequenceNumber = seq++;
            compl.SessionId = sessionId;
            compl.Checksum = new byte[] { 0x1, 0x2, 0x3, 0x4 };

            buffer = compl.Serialize();

            reply = pingSender.Send("hurtig.ninja", timeout, buffer, options);

            Console.WriteLine("Status: {0}", reply.Status);
            Console.WriteLine("Address: {0}", reply.Address.ToString());
            Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
            Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
            Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
            Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);

            Console.WriteLine("----------------------");

            Console.ReadLine();



        }
    }
}