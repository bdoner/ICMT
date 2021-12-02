using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Messages
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
