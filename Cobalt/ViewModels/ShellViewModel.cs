using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cobalt.ViewModels.Flyouts;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro.Controls.Dialogs;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace Cobalt.ViewModels
{    

    [Export(typeof(IShell))]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ShellViewModel : Conductor<IrcTabViewModel>, IDragSource, IDropTarget, IShell
    {
        private readonly BindableCollection<IFlyout> _flyoutCollection = new BindableCollection<IFlyout>();
        private readonly IFlyout _networksFlyout;
        private readonly BindableCollection<IrcTabViewModel> _tabs = new BindableCollection<IrcTabViewModel>();

        private readonly IWindowManager _windowManager;
        
        [ImportingConstructor]
        public ShellViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
            Activated += MainViewModel_Activated;
            _tabs.CollectionChanged += _tabs_CollectionChanged;
            _networksFlyout = new NetworksFlyoutViewModel(IoC.Get<IDialogCoordinator>()) { Parent = this };
        }

        private void _tabs_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => NewTabInstructionsVisible);
        }

        private async void MainViewModel_Activated(object sender, ActivationEventArgs e)
        {
            await Task.Yield();
            var tab = new IrcTabViewModel() {DisplayName = "Root"};
            tab.AddChild(new IrcTabViewModel() {DisplayName = "Child"});
            tab.AddChild(new IrcTabViewModel() {DisplayName = "Child2"});
            ActivateItem(tab);
            tab = new IrcTabViewModel() {DisplayName = "Root2"};
            tab.AddChild(new IrcTabViewModel() {DisplayName = "2Child"});
            ActivateItem(tab);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _flyoutCollection.Add(_networksFlyout);
        }


        public IObservableCollection<IFlyout> FlyoutCollection => _flyoutCollection;
        public IObservableCollection<IrcTabViewModel> Tabs => _tabs;

        #region Actions

        public bool CanJoinChannel()
        {
            return true;
        }

        public void JoinChannel()
        {
            ActivateItem(new IrcTabViewModel() {DisplayName = "Button"});
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
                if (source.IsChannel)
                {
                    if (target.IsChannel && source.ParentTab == target.ParentTab)
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
                foreach (var chan in item.Children)
                {
                    EnsureItem(chan);
                }
                if (!Tabs.Contains(item))
                    Tabs.Add(item);
                base.ActivateItem(item);
            }
        }

        public override void DeactivateItem(IrcTabViewModel item, bool close)
        {
            if (close)
            {
                if (item.IsChannel)
                {
                    var parent = Tabs.First(s => s == item.ParentTab);
                    parent.Children.Remove(item);
                }
                else
                {
                    foreach (var child in item.Children)
                    {
                        base.DeactivateItem(child, true);
                    }
                    Tabs.Remove(item);
                }
                ActivateItem(DetermineNextItem(item));
            }
            base.DeactivateItem(item, close);
        }

        private static IrcTabViewModel DetermineNextItem(IrcTabViewModel removed)
        {
            return removed?.ParentTab;
        }
    }
}
