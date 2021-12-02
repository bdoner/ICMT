using IMCT.Core.Helpers;

namespace IMCT.Core.Messages
{
    public class Message : IMessage
    {
        internal readonly ByteConsumer Consumer;
        protected static readonly IReadOnlyList<byte> _magic = new byte[] { 0x69, 0x63, 0x6d, 0x74 };
        private bool _isValid = true;

        private static byte[]? _sessionId;
        private static UInt32? _sequenceNumber;

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


        public Message(byte[] rawIcmpMessage)
        {
            Consumer = new ByteConsumer(rawIcmpMessage);
            if (!Consumer.SkipTo(_magic))
            {
                _isValid = false;
                return;
            }

            Magic = Consumer.Consume(4);

            var seqNumBytes = Consumer.Consume(4);
            SequenceNumber = BitConverter.ToUInt32(seqNumBytes);
            MessageType = (MessageType)Consumer.ConsumeSingle();
            SessionId = Consumer.Consume(4);

            // If set, validate.
            if (_sequenceNumber != null) _isValid = _sequenceNumber < SequenceNumber;

            // Update to latest
            _sequenceNumber = SequenceNumber;

            // If not set, set.
            if (_sessionId == null) _sessionId = SessionId;

            if (_isValid) // Don't validate an invalid message
                _isValid = Enumerable.SequenceEqual(SessionId, _sessionId);

        }

        internal Message()
        {

        }

        public bool IsValid()
        {
            if (MessageType == MessageType.Unknown) return false;
            if (!Enumerable.SequenceEqual(Magic, _magic)) return false;

            return _isValid;
        }

        public static void ResetSession()
        {
            _sessionId = null;
            _sequenceNumber = null;
        }

        public byte[] Serialize()
        {
            var buff = new List<byte>();

            buff.AddRange(Magic);
            var seq = BitConverter.GetBytes(SequenceNumber);
            buff.AddRange(seq);
            buff.Add((byte)MessageType);
            buff.AddRange(SessionId);

            return buff.ToArray();

        }
    }
}