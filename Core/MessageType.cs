using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public enum MessageType : byte
    {
        Unknown = 0,

        SetupMessage = 1,
        DataMessage = 2,
        CompletionMessage = 3
    }
}
