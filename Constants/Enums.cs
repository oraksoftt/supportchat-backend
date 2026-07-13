namespace SupportChat.Backend.Constants;

public enum ChatStatus : byte
{
    Waiting = 1,
    Active = 2,
    Closed = 3,
    Transferred = 4
}

public enum ChatPriority : byte
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum SenderType : byte
{
    Customer = 1,
    Agent = 2,
    System = 3
}

public enum MessageType : byte
{
    Text = 1,
    File = 2,
    System = 3
}

public enum NotificationType : byte
{
    NewChat = 1,
    ChatAssigned = 2,
    AgentOffline = 3,
    UnassignedChat = 4,
    RatingRequest = 5
}

public enum NotificationChannel : byte
{
    Email = 1,
    Push = 2,
    SMS = 3
}

public enum NotificationStatus : byte
{
    Pending = 1,
    Sent = 2,
    Failed = 3
}

public enum DataType : byte
{
    String = 1,
    Int = 2,
    Bool = 3,
    Json = 4
}