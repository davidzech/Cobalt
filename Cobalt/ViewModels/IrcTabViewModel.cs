using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Cobalt.ViewModels
{
    public class IrcTabViewModel : Screen
    {

        public IObservableCollection<IrcTabViewModel> Children { get; } = new BindableCollection<IrcTabViewModel>();


        public void AddChild(IrcTabViewModel child)
        {            
            child.ParentTab = this;
            Children.Add(child);
        }

        private IrcTabViewModel _parentTab;

        public IrcTabViewModel ParentTab
        {
            get { return _parentTab; }
            set
            {
                _parentTab = value;
                NotifyOfPropertyChange();
            }
        }

        object _selectedItem;
        
        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsChannel => ParentTab == null;
    }
}
