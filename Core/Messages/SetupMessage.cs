using System.Text;

namespace Core.Messages
{
    public class SetupMessage : Message, IMessage
    {
        /// <summary>
        /// 1 byte
        /// </summary>
        public byte FileNameLength { get; set; }

        /// <summary>
        /// n bytes. Depending on <see cref="FileNameLength"/>
        /// </summary>
        public string FileName { get; set; }

        public SetupMessage(byte[] rawIcmpMessage) : base(rawIcmpMessage)
        {
            FileNameLength = Consumer.ConsumeSingle();

            var fileNameBytes = Consumer.Consume(FileNameLength);
            FileName = Encoding.ASCII.GetString(fileNameBytes);
        }

        internal SetupMessage() : base()
        {

        }

        /// <summary>
        /// A new, empty message with it's MessageType set. 
        /// The message is invalid.
        /// </summary>
        /// <returns>A SetupMessage</returns>
        public static SetupMessage Empty()
        {
            SetupMessage msg = new();
            msg.Magic = Message._magic.ToArray();
            msg.MessageType = MessageType.SetupMessage;
            return msg;
        }

        public new byte[] Serialize()
        {
            var buff = new List<byte>();

            buff.AddRange(base.Serialize());
            buff.Add(FileNameLength);
            var fileName = Encoding.ASCII.GetBytes(FileName);
            buff.AddRange(fileName);

            return buff.ToArray();
        }

    }
}
