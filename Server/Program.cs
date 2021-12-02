
using Core;
using Core.Messages;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ICMPTransfer
{
    class Server
    {

        private static readonly IReadOnlyList<byte> MAGIC = new byte[] { (byte)'l', (byte)'e', (byte)'e', (byte)'t' };
        public static void Main()
        {
            Console.WriteLine("Hello, World!");

            Directory.CreateDirectory("received_files");

            Socket listener = new(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            listener.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
            listener.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 });

            //EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                Message.ResetSession(); // New session begun :) 

                byte[] rawMsg = new byte[4096];
                _ = listener.Receive(rawMsg);

                var message = new Message(rawMsg);
                if (!message.IsValid()) continue;
                if (message.MessageType != MessageType.SetupMessage) continue;

                //A setup message is expected and checked
                var setupMessage = new SetupMessage(rawMsg);

                using (var outStream = new FileStream(Path.Combine("received_files", setupMessage.FileName), FileMode.Create))
                {
                    using (var sw = new BinaryWriter(outStream))
                    {
                        while (true)
                        {
                            rawMsg = new byte[4096];
                            listener.Receive(rawMsg);

                            message = new Message(rawMsg);
                            if (!message.IsValid()) continue; // Repeat messages, corrupt or wrong session
                            if (message.MessageType == MessageType.SetupMessage) continue; // Only taking Completion and Data messages

                            if (message.MessageType == MessageType.DataMessage)
                            {
                                var dataMessage = new DataMessage(rawMsg);
                                sw.Write(dataMessage.Data);
                            }
                            else if (message.MessageType == MessageType.CompletionMessage)
                            {
                                // check CRC
                                break;
                            }
                        }
                    }
                }
                break;
            }

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }


}
