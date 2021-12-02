namespace ICMT.Core
{
    public enum MessageType : byte
    {
        Unknown = 0,

        SetupMessage = 1,
        DataMessage = 2,
        CompletionMessage = 3
    }
}
