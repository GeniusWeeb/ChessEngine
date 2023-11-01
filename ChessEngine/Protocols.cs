namespace ChessEngine;

public class Protocols  : IPCMessage
{
    public Protocols(string type, string incomingData , string move)
    {
        this.msgType = type;
        this.data = incomingData;
        this.toMove = move;
    }
   
}

public enum ProtocolTypes
{
    MOVE , GAMESTART , GAMEND , GAMESTATE , VALIDATE , INDICATE
}

public abstract class IPCMessage
{
    public string msgType { get;   set; }
    public string data {get;   set;
    }

    public string toMove { get; set; }
}



