namespace IMCT.Core.Messages
{
    public class CompletionMessage : Message, IMessage
    {
        /// <summary>
        /// 4 bytes. CRC-32 sum
        /// </summary>
        public byte[] Checksum { get; set; }

        public CompletionMessage(byte[] rawIcmpMessage) : base(rawIcmpMessage)
        {
            Checksum = Consumer.Consume(4);
        }


        internal CompletionMessage() : base()
        {

        }

        public static CompletionMessage Empty()
        {
            CompletionMessage msg = new();
            msg.Magic = Message._magic.ToArray();
            msg.MessageType = MessageType.CompletionMessage;
            return msg;
        }

        public new byte[] Serialize()
        {
            var buff = new List<byte>();

            buff.AddRange(base.Serialize());
            buff.AddRange(Checksum);

            return buff.ToArray();
        }
    }
}
