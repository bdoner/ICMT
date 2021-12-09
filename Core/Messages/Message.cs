using Core.Helpers;
using ICMT.Core.Helpers;

namespace ICMT.Core.Messages
{
    public class Message : IMessage
    {
        internal readonly ByteConsumer Consumer = null!;
        protected const uint _magic = 1_768_123_764; // new byte[] { 0x69, 0x63, 0x6d, 0x74 } in BE;
        protected static readonly IReadOnlyList<byte> _magicBytes = new byte[] { 0x69, 0x63, 0x6d, 0x74 };
        private bool _isValid = true;

        private static byte[]? _sessionId;
        private static uint? _sequenceNumber;

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
        public byte[] SessionId { get; set; } = null!;


        public Message(byte[] rawIcmpMessage)
        {
            Consumer = new ByteConsumer(rawIcmpMessage);
            if (!Consumer.SkipTo(_magicBytes))
            {
                _isValid = false;
                return;
            }

            var magicBytes = Consumer.Consume(4);
            Magic = BitConverter.ToUInt32(magicBytes);

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
            if (Magic != _magic) return false;

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

            var mseq = BitConverterHelper.GetBytes(Magic, Endianness.BigEndian);
            buff.AddRange(mseq);
            var seq = BitConverterHelper.GetBytes(SequenceNumber, Endianness.BigEndian);
            buff.AddRange(seq);
            buff.Add((byte)MessageType);
            buff.AddRange(SessionId);

            return buff.ToArray();

        }
    }
}