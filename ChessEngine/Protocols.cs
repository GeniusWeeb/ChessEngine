namespace ChessEngine;

public class Protocols  : IPCMessage
{
}

public enum ProtocolTypes
{
    MOVE , GAMESTART , GAMEND , GAMESTATE
}

public abstract class IPCMessage
{
    public string msgType { get; set; }
    public string data {get; set;
    }
}


