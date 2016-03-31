using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace Cobalt.Controls
{
    internal partial class ChatPresenter
    {
        public IEnumerable<MessageLine> MessagesSource
        {
            get { return (IEnumerable<MessageLine>) GetValue(MessagesSourceProperty); }
            set { SetValue(MessagesSourceProperty, value); }
        }

        public static readonly DependencyProperty MessagesSourceProperty = DependencyProperty.Register(
            "MessagesSource", typeof(IEnumerable<MessageLine>), typeof(ChatPresenter),
            new PropertyMetadata(new List<MessageLine>(), new PropertyChangedCallback(OnMessagesSourcePropertyChanged)));

        private static void OnMessagesSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ChatPresenter)?.OnMessagesSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        private void OnMessagesSourceChanged(IEnumerable oldSource, IEnumerable newSource)
        {
            var oldCollection = oldSource as INotifyCollectionChanged;
            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= Collection_CollectionChanged;
            }

            var newCollection = newSource as INotifyCollectionChanged;
            if (newCollection != null)
            {
                newCollection.CollectionChanged += Collection_CollectionChanged;
            }
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateAll();
        }

        public Typeface Typeface => null;
    }
}
