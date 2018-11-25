using socketDemonstration.Network.Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace socketDemonstration.Network.Serverside
{
    public class Server
    {
        public event EventHandler<StateChangedEventArgs<Server>> StateChanged;
        public event EventHandler<StateChangedEventArgs<ClientState>> ClientStateChanged;
        public event EventHandler<PacketReceivedEventArgs<ClientState>> ClientPacketReceived;

        public bool Listening
        {
            get { return m_Listening; }
            protected set { m_Listening = value; }
        }

        public IList<ClientState> Clientel
        {
            get { return m_Clientel; }
            protected set { m_Clientel = value; }
        }

        public Server(int maxConnections) : base()
        {
            Clientel = new List<ClientState>(maxConnections);
        }

        public void Listen(int port, int maxConcurrentConnections)
        {
            if (Listening)
                return;

            if (m_Socket == null)
                m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            bool listening = false;
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                m_Socket.Bind(endPoint);
                m_Socket.Listen(maxConcurrentConnections);
                listening = true;
            }
            finally
            {
                OnStateChanged(listening);
            }
        }

        private void OnStateChanged(bool listening)
        {
            Listening = listening;
            if (listening)
            {
                StartAccepting();
            }
            else
            {
                Close();
            }
            EventHandler<StateChangedEventArgs<Server>> handler = StateChanged;
            if (handler != null)
            {
                StateChangedEventArgs<Server> e = new StateChangedEventArgs<Server>(this, listening);
                handler(this, e);
            }
        }

        private void StartAccepting()
        {
            if (!Listening)
                return;

            m_Socket.BeginAccept(AcceptCallback, this);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Server server = (Server)ar.AsyncState;
            Socket socket = null;
            bool accepted = false;

            try
            {
                socket = server.m_Socket.EndAccept(ar);
                accepted = true;
            }
            finally
            {
                if (accepted)
                {
                    ClientState client = new ClientState();
                    client.StateChanged += OnClientStateChanged;
                    client.PacketReceived += OnClientPacketReceived;
                    client.InitializeSocket(socket);

                    //OnClientStateChanged(client, client.Connected);
                }
                else
                {
                    socket.Close();
                }
            }

            server.StartAccepting();
        }

        private void OnClientStateChanged(object sender, StateChangedEventArgs<ClientState> e)
        {
            OnClientStateChanged(e.Sender, e.Active);
        }

        private void OnClientStateChanged(ClientState state, bool connected)
        {
            if (connected)
            {
                lock (Clientel)
                {
                    if (!Clientel.Contains(state))
                    {
                        Clientel.Add(state);
                    }
                }
            }
            else
            {
                lock (Clientel)
                {
                    if (Clientel.Contains(state))
                    {
                        state.StateChanged -= OnClientStateChanged;
                        state.PacketReceived -= OnClientPacketReceived;
                        Clientel.Remove(state);
                    }
                }
            }

            EventHandler<StateChangedEventArgs<ClientState>> handler = ClientStateChanged;
            if (handler != null)
            {
                StateChangedEventArgs<ClientState> e = new StateChangedEventArgs<ClientState>(state, connected);
                handler(this, e);
            }
        }

        private void OnClientPacketReceived(object sender, PacketReceivedEventArgs<ClientState> e)
        {
            EventHandler<PacketReceivedEventArgs<ClientState>> handler = ClientPacketReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Close()
        {
            for (int i = 0; i < Clientel.Count; i++)
            {
                Clientel[i].Close();
                Clientel[i].StateChanged -= OnClientStateChanged;
                Clientel[i].PacketReceived -= OnClientPacketReceived;
                Clientel.Remove(Clientel[i]);
            }
            if (m_Socket != null)
                m_Socket.Close();
        }

        private bool m_Listening;
        private IList<ClientState> m_Clientel;
        private Socket m_Socket;
    }
}
