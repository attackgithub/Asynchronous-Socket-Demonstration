using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socketDemonstration.Network.Shared.Events
{
    public class StateChangedEventArgs<T>
    {
        public bool Active
        {
            get { return m_Active; }
            protected set { m_Active = value; }
        }

        public T Sender
        {
            get { return m_Sender; }
            protected set { m_Sender = value; }
        }

        public StateChangedEventArgs(T sender, bool active) : base()
        {
            Sender = sender;
            Active = active;
        }

        private T m_Sender;
        private bool m_Active;
    }
}
