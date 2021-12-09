namespace ICMT.Core.Messages
{
    internal interface IMessage
    {
        /// <summary>
        /// 4 bytes
        /// </summary>
        public uint Magic { get; set; }

        /// <summary>
        /// 4 bytes
        /// </summary>
        public uint SequenceNumber { get; set; }

        /// <summary>
        /// 1 byte
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// 4 bytes
        /// </summary>
        public byte[] SessionId { get; set; }

        public byte[] Serialize();
    }
}
