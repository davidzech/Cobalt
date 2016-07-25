using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Converters;
using Caliburn.Micro;
using Cobalt.Core.Irc;
using Cobalt.Settings;
using Cobalt.Settings.Elements;

namespace Cobalt.ViewModels
{
    public class IrcServerTabViewModel : IrcTabViewModel
    {
        private IEnumerable<Tuple<string, string>> AutoJoinChannels { get; }

        private IEnumerable<string> ConnectCommands { get; }

        private bool HasConnectedAlready { get; set; } = false;

        [ImportingConstructor]
        public IrcServerTabViewModel(ISettings settings, IrcConnection connection, IEnumerable<Tuple<string,string>> autoJoinChannels = null, IEnumerable<string> connectCommands = null) : base(settings, connection)
        {
            AutoJoinChannels = autoJoinChannels == null ? Enumerable.Empty<Tuple<string, string>>() : new List<Tuple<string, string>>(autoJoinChannels);

            ConnectCommands = connectCommands == null ? Enumerable.Empty<string>() : new List<string>();
        }

        public override bool IsChannel => false;

        public override bool IsServer => true;

        public IObservableCollection<IrcChannelTabViewModel> Children { get; } = new BindableCollection<IrcChannelTabViewModel>();

        public void AddChild(IrcChannelTabViewModel child)
        {
            child.ParentTab = this;
            Children.Add(child);
        }

        protected override void Connection_SelfJoined(object sender, IrcJoinEventArgs e)
        {
            base.Connection_SelfJoined(sender, e);
            var channelName = e.Channel.Name;
            // see if we have a child of this name already
            var child = Children.FirstOrDefault(c => c.ChannelName == channelName);
            if (child != null)
            {
                child.IsConnected = true;
            }
            else
            {
                // no child exists with this channel name, add a new child
                var channel = new IrcChannelTabViewModel(Settings, Connection, channelName) { DisplayName = channelName };
                AddChild(channel);                
            }
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
                    await Connection.JoinAsync(AutoJoinChannels);

                    // TODO execute commands
                    foreach (var cmd in ConnectCommands)
                    {
                        
                    }
                }
            }                 
        }
    }
}
