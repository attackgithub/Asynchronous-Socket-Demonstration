using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socketDemonstration.Network.Shared.Events
{
    public class PacketReceivedEventArgs<T>
    {
        public Packet Packet
        {
            get { return m_Packet; }
            protected set { m_Packet = value; }
        }

        public T Sender
        {
            get { return m_Sender; }
            protected set { m_Sender = value; }
        }

        public PacketReceivedEventArgs(T sender, Packet packet) : base()
        {
            Sender = sender;
            Packet = packet;
        }

        private T m_Sender;
        private Packet m_Packet;
    }
}
