using Core.Helpers;

namespace ICMT.Core.Messages
{
    public class DataMessage : Message, IMessage
    {
        /// <summary>
        /// 2 bytes
        /// </summary>
        public ushort DataLength { get; set; }

        /// <summary>
        /// n bytes, depending on <see cref="DataLength"/>
        /// </summary>
        public byte[] Data { get; set; } = null!;

        public DataMessage(byte[] rawIcmpMessage) : base(rawIcmpMessage)
        {
            var dataLengthBytes = Consumer.Consume(2);
            DataLength = BitConverter.ToUInt16(dataLengthBytes);

            Data = Consumer.Consume(DataLength);
        }

        internal DataMessage() : base()
        {
            
        }

        public static DataMessage Empty()
        {
            DataMessage msg = new();
            msg.Magic = Message._magic;
            msg.MessageType = MessageType.DataMessage;
            return msg;
        }

        public new byte[] Serialize()
        {
            var buff = new List<byte>();

            buff.AddRange(base.Serialize());
            var dl = BitConverterHelper.GetBytes(DataLength, Endianness.BigEndian);
            buff.AddRange(dl);
            buff.AddRange(Data);

            return buff.ToArray();
        }

        
    }
}
