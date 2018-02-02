namespace AuthServer.ServiceModel
{
    public enum RegisterResult : byte
    {
        OK,
        AlreadyExists,
        WrongKey
    }
}
