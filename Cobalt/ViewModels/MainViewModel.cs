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
    public class MainViewModel : PropertyChangedBase, IParent<IrcTabViewModel>, IDragSource, IDropTarget
    {

        private readonly BindableCollection<IrcTabViewModel> _channels = new BindableCollection<IrcTabViewModel>();
        public MainViewModel()
        {
            var tab = new IrcTabViewModel() { DisplayName = "Root" };
            tab.AddChild(new IrcTabViewModel() { DisplayName = "Child" });
            tab.AddChild(new IrcTabViewModel() { DisplayName = "Child2"});
            _channels.Add(tab);
            tab = new IrcTabViewModel() { DisplayName = "Root2" };
            _channels.Add(tab);
        }

        public IObservableCollection<IrcTabViewModel> Channels => _channels;


        IrcTabViewModel _activeItem;
        public IrcTabViewModel ActiveItem
        {
            get { return _activeItem; }
            set
            {
                _activeItem = value;
                NotifyOfPropertyChange();
            }
        }

        public bool CanJoinChannel()
        {            
            return true;
        }
        public void JoinChannel()
        {
            MessageBox.Show("HELLO");
        }

        public bool NewTabInstructionsVisible => Channels.Count == 0;
        public IEnumerable<IrcTabViewModel> GetChildren()
        {
            return Channels;
        }

        IEnumerable IParent.GetChildren()
        {
            return Channels;
        }

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
            dropInfo.DestinationText = "TEST";
            var source = dropInfo.DragInfo.SourceItem as IrcTabViewModel;
            var target = dropInfo.TargetItem as IrcTabViewModel;
            if (target != source && target?.IsChannel == source?.IsChannel && !dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
            {
                DragDrop.DefaultDropHandler.DragOver(dropInfo);
            }

        }

        public void Drop(IDropInfo dropInfo)
        {
            DragDrop.DefaultDropHandler.Drop(dropInfo);
        }
    }
}
