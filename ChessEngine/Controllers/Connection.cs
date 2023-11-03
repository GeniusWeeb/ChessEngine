


using System.Diagnostics;
using ChessEngine;
using Newtonsoft.Json;

namespace Utility{
    // Note : We are using Fleck package for bidirectional comms
    // It is a websocket

    using Fleck;
    public class Connection
    {
        private static List<IWebSocketConnection> connectedClient = new List<IWebSocketConnection>();
        private IWebSocketConnection mainSocket;

        
        public static Connection Instance { get; private set; }

        static Connection() {
            Instance = new Connection();
        }

        private Connection()
        {
            var server = new WebSocketServer("ws://127.0.0.1:8281");
            server.Start(
    
                socket =>
                {
                    socket.OnOpen = () => ClientConnected(socket);
                    socket.OnClose = () => ClientDC(socket);
                    socket.OnMessage += OnMessage;
                }
            );
          
        }
            
        public void Init()
        {
           // Console.WriteLine("The connection class is ready ");
        }

        #region Event handlers
        
        private void OnMessage(string data)
        {
            Protocols  incomingData= JsonConvert.DeserializeObject<Protocols>(data);
            EvaluateIncomingData(incomingData);
       
        }

        void ClientConnected(IWebSocketConnection socket)
        {
            connectedClient.Add(socket);
            mainSocket = socket;
            //Move this to wait till a game mode has been passed.
        }

        void ClientDC(IWebSocketConnection socket)
        {
           // Console.Write("Client has disconnected => " + socket.ConnectionInfo.Id);
           
            connectedClient.Remove(socket);
        }

          public void Send(string data)
            {
                mainSocket.Send(data);
            }



          private  void EvaluateIncomingData(Protocols incomingData)
          {
              
              if (incomingData.msgType == ProtocolTypes.INDICATE.ToString())
              {
                  Event.GetCellsForThisIndex.Invoke(Int32.Parse( incomingData.data));
                  return;
              }
              
              if (incomingData.msgType == ProtocolTypes.GAMEMODE.ToString())
              { 
                 
                  Event.ClientConncted.Invoke(incomingData.data);
                  return;

              }
          
              if (incomingData.msgType == ProtocolTypes.MOVE.ToString())
              {   
                 
                  Event.inComingData.Invoke(incomingData.data);
                  return;

              }

          }


          #endregion
    }

}










