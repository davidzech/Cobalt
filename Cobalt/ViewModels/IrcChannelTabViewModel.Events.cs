using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cobalt.Controls;
using Cobalt.Core.Irc;

namespace Cobalt.ViewModels
{
    public partial class IrcChannelTabViewModel
    {
        protected override void Connection_PrivateMessaged(object sender, IrcMessageEventArgs e)
        {
            base.Connection_PrivateMessaged(sender, e);
            if (!e.Handled && e.To.IsChannel && e.To.Name == this.ChannelName) 
            {
                for(int i = 0; i < 50; i++)
                Write(MessageType.Default, e.From, e.Text + i);
            }
        }
    }
}
