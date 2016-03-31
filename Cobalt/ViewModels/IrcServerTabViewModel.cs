using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cobalt.Core.Irc;
using Cobalt.Settings.Elements;

namespace Cobalt.ViewModels
{
    public class IrcServerTabViewModel : IrcTabViewModel
    {
        private IEnumerable<Tuple<string, string>> AutoJoinChannels { get; }

        private IEnumerable<string> ConnectCommands { get; }

        private bool HasConnectedAlready { get; set; } = false;

        public IrcServerTabViewModel(IrcConnection connection, IEnumerable<Tuple<string,string>> autoJoinChannels = null, IEnumerable<string> connectCommands = null) : base(connection)
        {
            AutoJoinChannels = autoJoinChannels;
            ConnectCommands = connectCommands;
        }

        protected override async void Connection_StateChanged(object sender, EventArgs e)
        {
            base.Connection_StateChanged(sender, e);
            var state = this.Connection.State;

            if (!HasConnectedAlready)
            {
                if (state == IrcConnectionState.Connected)
                {
                    // this is first on connect
                    HasConnectedAlready = true;

                    foreach (var channelPair in AutoJoinChannels)
                    {
                        var channel = channelPair.Item1;
                        var password = channelPair.Item2;
                        if (!string.IsNullOrEmpty(password))
                        {
                            await Connection.JoinAsync(channel, password);
                        }
                        else
                        {
                            await Connection.JoinAsync(channel);
                        }
                    }
                }
            }                 
        }
    }
}
