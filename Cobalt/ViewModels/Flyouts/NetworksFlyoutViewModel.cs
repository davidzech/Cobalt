using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Cobalt.Settings.Elements;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Cobalt.ViewModels.Flyouts
{
    public class NetworksFlyoutViewModel : FlyoutViewModelBase
    {
        public override string Header
        {
            get { return "Networks"; }
            set
            {
            }
        }

        private readonly IDialogCoordinator _coordinator;
        [ImportingConstructor]
        public NetworksFlyoutViewModel(IDialogCoordinator coordinator)
        {
            _coordinator = coordinator;
            SelectedNetwork = Networks.FirstOrDefault();
        }

        public IObservableCollection<NetworkElement> Networks => App.Settings.RootElement.Networks;

        private NetworkElement _selectedNetwork;
        public NetworkElement SelectedNetwork
        {
            get { return _selectedNetwork; }
            set
            {
                if (_selectedNetwork != value)
                {
                    _selectedNetwork = value;
                    NotifyOfPropertyChange();
                }
            }            
        }

        private ChannelElement _selectedChannel;

        public ChannelElement SelectedChannel
        {
            get { return _selectedChannel; }
            set
            {
                if (_selectedChannel != value)
                {
                    _selectedChannel = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanAddChannel()
        {
            return _selectedNetwork != null;
        }

        public void AddChannel()
        {
            var add = new ChannelElement() {Name = "#"};
            _selectedNetwork?.Channels.Add(add);
            SelectedChannel = add;
        }

        public bool CanRemoveChannel()
        {
            return _selectedChannel != null;
        }

        public void RemoveChannel()
        {
            _selectedNetwork?.Channels?.Remove(_selectedChannel);
            _selectedChannel = _selectedNetwork?.Channels?.FirstOrDefault();
        }

        public void AddNetwork()
        {
            var network = new NetworkElement() { Name = "New Server"};        
            Networks.Add(network);
            SelectedNetwork = network;
        }

        public bool CanRemoveNetwork()
        {
            return SelectedNetwork != null;
        }

        public async void RemoveNetwork()
        {

            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Delete",
                NegativeButtonText = "Cancel"
            };

            var result = await _coordinator.ShowMessageAsync(Parent, "Remove Network?", $"Are you sure you want to remove the network: {SelectedNetwork.Name}?", MessageDialogStyle.AffirmativeAndNegative, mySettings);
            if (result == MessageDialogResult.Affirmative)
            {
                Networks.Remove(SelectedNetwork);
                SelectedNetwork = Networks.FirstOrDefault();
            }
        }

        public override Position Position
        {
            get
            {
                return Position.Left;
            }
            set
            {
            }
        }
    }
}
