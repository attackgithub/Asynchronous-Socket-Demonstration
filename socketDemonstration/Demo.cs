using socketDemonstration.Network.Clientside;
using socketDemonstration.Network.Serverside;
using socketDemonstration.Network.Shared;
using socketDemonstration.Network.Shared.Events;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace socketDemonstration
{
    public static class Demo
    {

        public static int Main(string[] argv)
        {
            m_Server = new Server(1000);
            m_Server.ClientStateChanged += OnClientStateChanged;
            m_Server.ClientPacketReceived += OnClientPacketReceived;
            m_Server.StateChanged += OnServerStateChanged;
            m_Server.Listen(33050, 100);

            Client client = CreateFakeClient();

            Process.GetCurrentProcess().WaitForExit();

            return 0;
        }

        private static Client CreateFakeClient()
        {
            Client fakeClient = new Client();
            fakeClient.StateChanged += (sender, connected) =>
             {
                 Client client = (Client)sender;
                 client.Send(Packet.Create(Encoding.UTF8.GetBytes("Hey there, server!")));
                 Thread.Sleep(1000);
                 client.Close();
             };
            fakeClient.Connect("localhost", 33050);
            return fakeClient;
        }

        private static void OnServerStateChanged(object sender, StateChangedEventArgs<Server> e)
        {
                if (e.Active)
                {
                    Console.WriteLine("Server is now listening");
                }
                else
                {
                    Console.WriteLine("Server stopped listening");
                }
        }

        private static void OnClientStateChanged(object sender, StateChangedEventArgs<ClientState> e)
        {
                if (e.Active)
                {
                    Console.WriteLine("A new client has connected");
                }
                else
                {
                    Console.WriteLine("A client has disconnected");
                }

                string numClients = (sender as Server).Clientel.Count.ToString();
                Console.WriteLine("Current amount of clients: {0}", numClients);
        }

        private static void OnClientPacketReceived(object sender, PacketReceivedEventArgs<ClientState> e)
        {
                Console.WriteLine("A client said: {0}", Encoding.UTF8.GetString(e.Packet.Data));
        }

        private static Server m_Server;
    }
}
