namespace IMCT.Core.Messages
{
    internal interface IMessage
    {
        /// <summary>
        /// 4 bytes
        /// </summary>
        public byte[] Magic { get; set; }

        /// <summary>
        /// 4 bytes
        /// </summary>
        public UInt32 SequenceNumber { get; set; }

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
