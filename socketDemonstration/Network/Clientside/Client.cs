using socketDemonstration.Network.Shared;
using socketDemonstration.Network.Shared.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace socketDemonstration.Network.Clientside
{
    public class Client
    {
        public event EventHandler<StateChangedEventArgs<Client>> StateChanged;
        public event EventHandler<PacketReceivedEventArgs<Client>> PacketReceived;

        public bool Connected
        {
            get { return m_Connected; }
            protected set { m_Connected = value; }
        }

        public Client() : base()
        {
            m_Buffer = new byte[8];
        }

        public void Connect(string host, int port)
        {
            if (Connected)
                return;

            if (m_Socket == null)
                m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            m_Socket.BeginConnect(host, port, ConnectCallback, this);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;
            bool connected = false;
            try
            {
                client.m_Socket.EndConnect(ar);
                connected = true;
            }
            finally
            {
                client.OnStateChanged(connected);
            }
        }

        private void OnStateChanged(bool connected)
        {
            Connected = connected;
            if (connected)
            {
                m_Closed = false;
                StartReceiving();
            }
            EventHandler<StateChangedEventArgs<Client>> handler = StateChanged;
            if (handler != null)
            {
                StateChangedEventArgs<Client> e = new StateChangedEventArgs<Client>(this, connected);
                handler(this, e);
            }
        }

        private void StartReceiving()
        {
            if (!Connected)
                return;

            m_Socket.BeginReceive(m_Buffer, 0, m_Buffer.Length, SocketFlags.None, ReceiveCallback, this);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;
            int amountReceived = 0, headerSignature = 0, amountToReceive = 0;
            Packet packet = null;
            bool success = false;

            try
            {
                amountReceived = client.m_Socket.EndReceive(ar);

                if (amountReceived > 0)
                {
                    if (amountReceived == 8)
                    {
                        headerSignature = Conversions.FromBytes(client.m_Buffer, 0);
                        amountToReceive = Conversions.FromBytes(client.m_Buffer, 4);

                        if (headerSignature == 0xBEEF)
                        {
                            success = true;
                            if (amountToReceive > 0)
                            {
                                byte[] payloadBuffer = new byte[amountToReceive];

                                IAsyncResult ar2 = client.m_Socket.BeginReceive(payloadBuffer, 0, payloadBuffer.Length, SocketFlags.None, null, null);
                                amountReceived = client.m_Socket.EndReceive(ar2);

                                if (amountReceived == amountToReceive)
                                {
                                    packet = Packet.Create(payloadBuffer);
                                }
                                else
                                {
                                    success = false;
                                }
                            }
                        }
                    }
                }
            } catch (Exception e)
            {
                success = false;
            }
            finally
            {
                if (success)
                {
                    client.OnPacketReceieved(client, packet);
                    client.StartReceiving();
                }
                else
                {
                    client.OnStateChanged(false);
                }
            }
        }

        private void OnPacketReceieved(Client client, Packet packet)
        {
            EventHandler<PacketReceivedEventArgs<Client>> handler = PacketReceived;
            if (handler != null)
            {
                PacketReceivedEventArgs<Client> e = new PacketReceivedEventArgs<Client>(client, packet);
                handler(this, e);
            }
        }

        public void Send(Packet packet)
        {
            if (!Connected)
                return;

            byte[] serialized = packet.Serialize();
                m_Socket.BeginSend(serialized, 0, serialized.Length, SocketFlags.None, SendCallback, this);

        }

        private void SendCallback(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;
            int sent = 0;
            bool success = false;
            try
            {
                    sent = client.m_Socket.EndSend(ar);
                success = true;
            } catch (Exception e )
            {
                success = false;
            }
            finally
            {
                if (!success)
                {
                    client.OnStateChanged(false);
                }
            }
        }

        public void Close()
        {
            if (m_Closed)
                return;

            if (m_Socket != null)
                m_Socket.Close();

            m_Closed = true;

            OnStateChanged(false);
        }

        private Socket m_Socket;
        private byte[] m_Buffer;
        private bool m_Connected;
        private bool m_Closed;
    }
}
