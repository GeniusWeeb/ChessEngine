namespace ChessEngine;

public class Protocols  : IPCMessage
{
    public Protocols(string type, string incomingData)
    {
        this.msgType = type;
        this.data = incomingData;
    }
   
}

public enum ProtocolTypes
{
    MOVE , GAMESTART , GAMEND , GAMESTATE , VALIDATE
}

public abstract class IPCMessage
{
    public string msgType { get;   set; }
    public string data {get;   set;
    }
}


