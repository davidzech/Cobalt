using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }

        private void UnsubscribeIrcEvents()
        {
            
        }

        private void Connection_PrivateMessaged(object sender, Core.Irc.IrcMessageEventArgs e)
        {
            // check if ignored 

            if (!IsServer)
            {
                if (IsChannel)
                {
                    // TODO Handle color and attention
                    Write(MessageType.Default, e.From, e.Text);
                }
            }
        }



        private void Connection_Noticed(object sender, Core.Irc.IrcMessageEventArgs e)
        {            
            throw new NotImplementedException();
        }

        private void Connection_ConnectionError(object sender, Core.Irc.ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Connection_StateChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();           
        }

        private void Write(MessageType type, string nick, string text, bool attention = false, int colorHashCode = 0)
        {
            MessageLine ml = MessageLine.Process(type, MessageMarker.None, nick, text, DateTime.Now);
            Messages.Add(ml);
            while(Messages.Count > BufferLines)
            {
                Messages.RemoveAt(0);   
            }
        }

        private void Write(MessageType type, IrcPrefix from, string text, bool attention = false, int colorHashCode = 0)
        {
            Write(type, from.Prefix, text, attention, colorHashCode);
        }
    }
}
