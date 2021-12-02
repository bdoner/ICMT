using ICMT.Core.Helpers;
using ICMT.Core;
using ICMT.Core.Messages;
using System.Net;
using System.Net.Sockets;

namespace ICMT
{
    class Server
    {
        public static void Main()
        {

            Directory.CreateDirectory("received_files");
            Console.WriteLine("Files can be found in \"./received_files/\"");
            Socket socket = new(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);

            var lip = IPAddress.Parse("127.0.0.1");

            socket.Bind(new IPEndPoint(lip, 0));
            socket.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 });

            Console.WriteLine($"Bound listener.");

            EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                Message.ResetSession(); // New session begun :) 

                Console.WriteLine("Listening for first message");

                byte[] rawMsg = new byte[4096];
                _ = socket.ReceiveFrom(rawMsg, ref ep);

                var message = new Message(rawMsg);
                Console.WriteLine($"Message of type {message.MessageType} received.");

                if (!message.IsValid()) continue;
                if (message.MessageType != MessageType.SetupMessage) continue;

                //A setup message is expected and checked
                var setupMessage = new SetupMessage(rawMsg);
                var newFile = Path.Combine("received_files", setupMessage.FileName);
                Console.WriteLine($"Writing data to file {newFile}");
                using (var outStream = new FileStream(newFile, FileMode.Create))
                {
                    using (var sw = new BinaryWriter(outStream))
                    {
                        while (true)
                        {
                            rawMsg = new byte[4096];
                            socket.Receive(rawMsg);

                            message = new Message(rawMsg);
                            if (!message.IsValid()) continue; // Repeat messages, corrupt or wrong session
                            if (message.MessageType == MessageType.SetupMessage) continue; // Only taking Completion and Data messages

                            if (message.MessageType == MessageType.DataMessage)
                            {
                                DataMessage dataMessage = new(rawMsg);
                                sw.Write(dataMessage.Data);

                                //Console.WriteLine($"Wrote {dataMessage.DataLength} bytes to file");
                            }
                            else if (message.MessageType == MessageType.CompletionMessage)
                            {
                                sw.Flush();
                                sw.Close();

                                CompletionMessage complMsg = new(rawMsg);
                                var checksum = ChecksumHelper.GetFileChecksum(newFile);

                                if (!Enumerable.SequenceEqual(checksum, complMsg.Checksum))
                                {
                                    Console.WriteLine("WARN: Checksum of local file does not match sent file.");
                                }
                                Console.WriteLine("Checksum for received file is:\t0x" + BitConverter.ToUInt32(complMsg.Checksum).ToString("X2"));
                                Console.WriteLine("Checksum for sent file is:\t0x" + BitConverter.ToUInt32(checksum).ToString("X2"));

                                break;
                            }
                        }
                    }
                }
                //break;
            }

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }


}
