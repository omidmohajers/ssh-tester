namespace PA.SSH
{
    public enum StatusType
    {
        Message = 0,
        Done = 1,
        Error = 2,
        Exception = 3,
        ReplyButNoAthenticate = 4,
        PingOK = 5,
        PingError = 6,
    }
}