using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interactivity;
using Caliburn.Micro;
using Cobalt.Behaviors;
using GongSolutions.Wpf.DragDrop;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace Cobalt.ViewModels
{    

    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainViewModel : Conductor<IrcTabViewModel>, IDragSource, IDropTarget
    {

        private readonly BindableCollection<IrcTabViewModel> _tabs = new BindableCollection<IrcTabViewModel>();
        public MainViewModel()
        {
            this.Activated += MainViewModel_Activated;
            _tabs.CollectionChanged += _tabs_CollectionChanged;
        }

        private void _tabs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => this.NewTabInstructionsVisible);
        }

        private async void MainViewModel_Activated(object sender, ActivationEventArgs e)
        {
            await Task.Yield();
            var tab = new IrcTabViewModel() { DisplayName = "Root" };
            tab.AddChild(new IrcTabViewModel() { DisplayName = "Child" });
            tab.AddChild(new IrcTabViewModel() { DisplayName = "Child2" });
            ActivateItem(tab);
            tab = new IrcTabViewModel() { DisplayName = "Root2" };
            tab.AddChild(new IrcTabViewModel() {DisplayName = "2Child"});
            ActivateItem(tab);
        }

        public IObservableCollection<IrcTabViewModel> Tabs => _tabs;

        public bool CanJoinChannel()
        {            
            return true;
        }
        public void JoinChannel()
        {
            ActivateItem(new IrcTabViewModel() {DisplayName = "Button"});
        }

        public bool NewTabInstructionsVisible => Tabs.Count == 0;

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

            if (target != source && target.IsChannel == source.IsChannel && !dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
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
                    base.EnsureItem(chan);
                }
                if(!Tabs.Contains(item))
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

        private IrcTabViewModel DetermineNextItem(IrcTabViewModel removed)
        {
            return removed?.ParentTab;
        }

        public void Drop(IDropInfo dropInfo)
        {
            DragDrop.DefaultDropHandler.Drop(dropInfo);
        }
    }
}
