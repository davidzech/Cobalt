using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cobalt.Core.Irc;
using Cobalt.Settings;
using Cobalt.Settings.Elements;

namespace Cobalt.ViewModels
{
    public partial class IrcChannelTabViewModel : IrcTabViewModel
    {
        private IrcServerTabViewModel _parentTab;

        public IrcServerTabViewModel ParentTab
        {
            get { return _parentTab; }
            set
            {
                _parentTab = value;
                NotifyOfPropertyChange();
            }
        }

        public string ChannelName
        {
            get;
        }

        public bool CanAutoJoin()
        {
            return !string.IsNullOrEmpty(ParentTab.UniqueIdentifier);
        }

        private bool _autoJoin;
        public bool AutoJoin
        {
            get { return _autoJoin; }
            set
            {
                _autoJoin = value;                                
                NotifyOfPropertyChange();
                var parentUid = ParentTab.UniqueIdentifier;
                var network = Settings.RootElement.Networks.FirstOrDefault(n => n.UniqueIdentifier == parentUid);
                if (network != null)
                {
                    var cElement = network.Channels.FirstOrDefault(c => c.Name == ChannelName);
                    if (cElement == null && value == true)
                    {
                        network.Channels.Add(new ChannelElement() { Name = ChannelName});
                    }
                    else if (cElement != null && value == false)
                    {
                        network.Channels.Remove(cElement);
                    }
                }
            }
        }

        [ImportingConstructor]
        public IrcChannelTabViewModel(ISettings settings, IrcConnection connection, string channelName) : base(settings, connection)
        {
            ChannelName = channelName;
        }

        public Task JoinAsync()
        {
            return Connection.JoinAsync(ChannelName);
        }

        public override bool IsChannel => true;
        public override bool IsServer => false;
    }
}
