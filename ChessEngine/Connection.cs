
using System.ComponentModel;
using System.Net.WebSockets;

namespace Utility{
    // Note : We are using Fleck package for bidirectional comms
    // It is a websocket

    using Fleck;
    public class Connection
    {
        private static List<IWebSocketConnection> connectedClient = new List<IWebSocketConnection>();
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
            Console.WriteLine("The connection class is ready ");
        }

        #region Event handlers
        
        private void OnMessage(string data)
        {
            Console.WriteLine(data);
        }

        void ClientConnected(IWebSocketConnection socket)
        {
            connectedClient.Add(socket);

            Console.WriteLine("Client has connected => " + socket.ConnectionInfo.Id);
        }

        void ClientDC(IWebSocketConnection socket)
        {
            Console.Write("Client has disconnected => " + socket.ConnectionInfo.Id);

            connectedClient.Remove(socket);
        }

        #endregion
    }

}










