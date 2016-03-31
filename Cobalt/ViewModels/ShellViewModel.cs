using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cobalt.Settings;
using Cobalt.ViewModels.Flyouts;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro.Controls.Dialogs;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace Cobalt.ViewModels
{    

    [Export(typeof(IShell))]
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class ShellViewModel : Conductor<IrcTabViewModel>, IDragSource, IDropTarget, IShell
    {
        private readonly BindableCollection<IFlyout> _flyoutCollection = new BindableCollection<IFlyout>();
        private readonly IFlyout _networksFlyout;
        private readonly BindableCollection<IrcServerTabViewModel> _tabs = new BindableCollection<IrcServerTabViewModel>();

        private readonly IWindowManager _windowManager;
        
        [ImportingConstructor]
        public ShellViewModel(IWindowManager windowManager, IDialogCoordinator coordinator, ISettings settings)
        {
            _windowManager = windowManager;
            Activated += MainViewModel_Activated;
            _tabs.CollectionChanged += _tabs_CollectionChanged;
            _networksFlyout = new NetworksFlyoutViewModel(coordinator, settings) { Parent = this };
        }

        private void _tabs_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => NewTabInstructionsVisible);
        }

        private async void MainViewModel_Activated(object sender, ActivationEventArgs e)
        {
            await Task.Yield();
            /*
            var tab = new IrcTabViewModel() {DisplayName = "Root"};
            tab.AddChild(new IrcTabViewModel() {DisplayName = "Child"});
            tab.AddChild(new IrcTabViewModel() {DisplayName = "Child2"});
            ActivateItem(tab);
            tab = new IrcTabViewModel() {DisplayName = "Root2"};
            tab.AddChild(new IrcTabViewModel() {DisplayName = "2Child"});
            ActivateItem(tab);
            */
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _flyoutCollection.Add(_networksFlyout);
        }


        public IObservableCollection<IFlyout> FlyoutCollection => _flyoutCollection;
        public IObservableCollection<IrcServerTabViewModel> Tabs => _tabs;

        #region Actions

        public bool CanJoinChannel()
        {
            return true;
        }

        public void JoinChannel()
        {
        }
    
        public void ToggleNetworksFlyout()
        {
            _networksFlyout.IsOpen = !_networksFlyout.IsOpen;
        }

        #endregion

        #region Properties   

        public bool NewTabInstructionsVisible => Tabs.Count == 0;

        #endregion

        #region Drag
        public void StartDrag(IDragInfo dragInfo)
        {
            DragDrop.DefaultDragHandler.StartDrag(dragInfo);
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return DragDrop.DefaultDragHandler.CanStartDrag(dragInfo);
        }

        public void Dropped(IDropInfo dropInfo)
        {

        }

        public void DragCancelled()
        {
            DragDrop.DefaultDragHandler.DragCancelled();
        }

        public void DragOver(IDropInfo dropInfo)
        {
            var source = dropInfo.DragInfo.SourceItem as IrcTabViewModel;
            var target = dropInfo.TargetItem as IrcTabViewModel;
            if (source == null || target == null)
                return;

            if (target != source && target.IsChannel == source.IsChannel &&
                !dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
            {
                if (source.IsChannel && target.IsChannel)
                {
                    IrcChannelTabViewModel ctarget = target as IrcChannelTabViewModel;
                    IrcChannelTabViewModel csource = source as IrcChannelTabViewModel;
                    if (ctarget.ParentTab == csource.ParentTab)
                    {
                        DragDrop.DefaultDropHandler.DragOver(dropInfo);
                    }
                }
                else
                {
                    DragDrop.DefaultDropHandler.DragOver(dropInfo);
                }
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            DragDrop.DefaultDropHandler.Drop(dropInfo);
        }

        #endregion
        public override void ActivateItem(IrcTabViewModel item)
        {
            if (item == null)
            {
                return;
            }
            if (item.IsChannel)
            {
                base.ActivateItem(item);
            }
            else
            {
                var server = (IrcServerTabViewModel) item;
                foreach (var chan in server.Children)
                {
                    EnsureItem(chan);
                }
                if (!Tabs.Contains(server))
                    Tabs.Add(server);
                base.ActivateItem(server);
            }
        }

        public override void DeactivateItem(IrcTabViewModel item, bool close)
        {
            if (close)
            {
                if (item.IsChannel)
                {
                    var citem = (IrcChannelTabViewModel) item;
                    var parent = Tabs.First(s => s == citem.ParentTab);
                    parent.Children.Remove(citem);
                }
                else
                {
                    var server = (IrcServerTabViewModel) item;
                    foreach (var child in server.Children)
                    {
                        base.DeactivateItem(child, true);
                    }
                    Tabs.Remove(server);
                }
                ActivateItem(DetermineNextItem(item));
            }
            base.DeactivateItem(item, close);
        }

        private static IrcTabViewModel DetermineNextItem(IrcTabViewModel removed)
        {
            return (removed as IrcChannelTabViewModel)?.ParentTab;
        }
    }
}
