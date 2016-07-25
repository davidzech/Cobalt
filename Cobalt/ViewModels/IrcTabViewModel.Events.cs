using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cobalt.Controls;
using Cobalt.Core.Irc;

namespace Cobalt.ViewModels
{
    public partial class IrcTabViewModel
    {
        private void SubscribeIrcEvents()
        {
            Connection.StateChanged += Connection_StateChanged;
            Connection.ConnectionError += Connection_ConnectionError;
            Connection.Noticed += Connection_Noticed;
            Connection.PrivateMessaged += Connection_PrivateMessaged;
            Connection.SelfJoined += Connection_SelfJoined;
            Connection.InfoReceived += Connection_InfoReceived;
        }

        protected virtual void Connection_InfoReceived(object sender, IrcInfoEventArgs e)
        {
        }

        protected virtual void Connection_SelfJoined(object sender, IrcJoinEventArgs e)
        {
        }

        private void UnsubscribeIrcEvents()
        {
            Connection.StateChanged -= Connection_StateChanged;
            Connection.ConnectionError -= Connection_ConnectionError;
            Connection.Noticed -= Connection_Noticed;
            Connection.PrivateMessaged -= Connection_PrivateMessaged;
            Connection.SelfJoined -= Connection_SelfJoined;            
        }

        protected virtual void Connection_PrivateMessaged(object sender, Core.Irc.IrcMessageEventArgs e)
        {
            // check if ignored            
        }

        protected virtual void Connection_Noticed(object sender, Core.Irc.IrcMessageEventArgs e)
        {            
        }

        protected virtual void Connection_ConnectionError(object sender, Core.Irc.ErrorEventArgs e)
        {
        }

        protected virtual void Connection_StateChanged(object sender, EventArgs e)
        {
                var state = Connection.State;
                if (state != IrcConnectionState.Connected)
                {
                    // TODO change notify state
                    this.IsConnected = false;
                }
         
        }

        public void Write(MessageType type, string nick, string text, bool attention = false, int colorHashCode = 0)
        {
            if (nick == null)
            {
                nick = "";
            }
            MessageLine ml = MessageLine.Process(type, MessageMarker.None, nick, text, DateTime.Now);
            Messages.Add(ml);
            while(Messages.Count > BufferLines)
            {
                Messages.RemoveAt(0);   
            }
        }

        public void Write(MessageType type, IrcPeer from, string text, bool attention = false, int colorHashCode = 0)
        {
            Write(type, from?.Nickname, text, attention, colorHashCode);
        }
    }
}
