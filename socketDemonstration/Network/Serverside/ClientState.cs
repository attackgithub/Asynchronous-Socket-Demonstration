using socketDemonstration.Network.Shared;
using socketDemonstration.Network.Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace socketDemonstration.Network.Serverside
{
    public class ClientState
    {
        public event EventHandler<StateChangedEventArgs<ClientState>> StateChanged;
        public event EventHandler<PacketReceivedEventArgs<ClientState>> PacketReceived;

        public bool Connected
        {
            get { return m_Connected; }
            protected set { m_Connected = value; }
        }

        public ClientState() : base()
        {
            m_Buffer = new byte[8];
        }

        public void InitializeSocket (Socket s)
        {
            if (s != null)
            {
                m_Socket = s;
                OnStateChanged(s.Connected);
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
            else
            {
                    Close();
            }
            EventHandler<StateChangedEventArgs<ClientState>> handler = StateChanged;
            if (handler != null)
            {
                StateChangedEventArgs<ClientState> e = new StateChangedEventArgs<ClientState>(this, connected);
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
            ClientState state = (ClientState)ar.AsyncState;
            int amountReceived = 0, headerSignature = 0, amountToReceive = 0;
            Packet packet = null;
            bool success = false;

            try
            {
                amountReceived = state.m_Socket.EndReceive(ar);
                if (amountReceived > 0)
                {
                    if (amountReceived == 8)
                    {
                        headerSignature = Conversions.FromBytes(state.m_Buffer, 0);
                        amountToReceive = Conversions.FromBytes(state.m_Buffer, 4);

                        if (headerSignature == 0xBEEF)
                        {
                            success = true;
                            if (amountToReceive > 0)
                            {
                                byte[] payloadBuffer = new byte[amountToReceive];

                                IAsyncResult ar2 = state.m_Socket.BeginReceive(payloadBuffer, 0, payloadBuffer.Length, SocketFlags.None, null, null);
                                amountReceived = state.m_Socket.EndReceive(ar2);

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
                    state.OnPacketReceieved(state, packet);
                    state.StartReceiving();
                }
                else
                {
                    state.OnStateChanged(false);
                }
            }
        }

        private void OnPacketReceieved(ClientState state, Packet packet)
        {
            EventHandler<PacketReceivedEventArgs<ClientState>> handler = PacketReceived;
            if (handler != null)
            {
                PacketReceivedEventArgs<ClientState> e = new PacketReceivedEventArgs<ClientState>(state, packet);
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
            ClientState state = (ClientState)ar.AsyncState;
            int sent = 0;
            bool success = false;
            try
            {
                sent = state.m_Socket.EndSend(ar);
                success = true;
            }
            catch (Exception e)
            {
                success = false;
            }
            finally
            {
                if (!success)
                {
                    state.OnStateChanged(false);
                }
            }
        }

        public void Close()
        {
            if (m_Closed)
                return;

            if (m_Socket != null)
                m_Socket.Close();

            if (m_Buffer != null)
                m_Buffer  = null;
            
            m_Closed = true;

            OnStateChanged(false);
        }

        private Socket m_Socket;
        private byte[] m_Buffer;
        private bool m_Connected;
        private bool m_Closed;
    }
}
