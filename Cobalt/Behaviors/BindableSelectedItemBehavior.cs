using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace Cobalt.Behaviors
{
    public class BindableSelectedItemBehavior : Behavior<TreeView>
    {
        #region SelectedItem Property

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        ///     Identifies the <see cref="SelectedItem" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(object),
                typeof(BindableSelectedItemBehavior),
                new UIPropertyMetadata(null, OnSelectedItemChanged));


        private static Action<int> GetBringIndexIntoView(Panel itemsHostPanel)
        {
            var virtualizingPanel = itemsHostPanel as VirtualizingStackPanel;

            var method = virtualizingPanel?.GetType().GetMethod(
                "BringIndexIntoView",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new[] { typeof(int) },
                null);
            if (method == null)
            {
                return null;
            }

            return i => method.Invoke(virtualizingPanel, new object[] { i });
        }

        /// <summary>
        /// Recursively search for an item in this subtree.
        /// </summary>
        /// <param name="container">
        /// The parent ItemsControl. This can be a TreeView or a TreeViewItem.
        /// </param>
        /// <param name="item">
        /// The item to search for.
        /// </param>
        /// <returns>
        /// The TreeViewItem that contains the specified item.
        /// </returns>
        private static TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            if (container != null)
            {
                if (container.DataContext == item)
                {
                    return container as TreeViewItem;
                }

                // Expand the current container
                if (container is TreeViewItem && !((TreeViewItem)container).IsExpanded)
                {
                    container.SetValue(TreeViewItem.IsExpandedProperty, true);
                }

                // Try to generate the ItemsPresenter and the ItemsPanel.
                // by calling ApplyTemplate.  Note that in the 
                // virtualizing case even if the item is marked 
                // expanded we still need to do this step in order to 
                // regenerate the visuals because they may have been virtualized away.
                container.ApplyTemplate();
                var itemsPresenter =
                    (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter != null)
                {
                    itemsPresenter.ApplyTemplate();
                }
                else
                {
                    // The Tree template has not named the ItemsPresenter, 
                    // so walk the descendents and find the child.
                    itemsPresenter = container.GetVisualDescendent<ItemsPresenter>();
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();
                        itemsPresenter = container.GetVisualDescendent<ItemsPresenter>();
                    }
                }

                var itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

                // Ensure that the generator for this panel has been created.
#pragma warning disable 168
                // ReSharper disable once UnusedVariable
                var children = itemsHostPanel.Children;
#pragma warning restore 168

                var bringIndexIntoView = GetBringIndexIntoView(itemsHostPanel);
                for (int i = 0, count = container.Items.Count; i < count; i++)
                {
                    TreeViewItem subContainer;
                    if (bringIndexIntoView != null)
                    {
                        // Bring the item into view so 
                        // that the container will be generated.
                        bringIndexIntoView(i);
                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                                                    ContainerFromIndex(i);
                    }
                    else
                    {
                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                                                    ContainerFromIndex(i);

                        // Bring the item into view to maintain the 
                        // same behavior as with a virtualizing panel.
                        subContainer.BringIntoView();
                    }

                    if (subContainer == null)
                    {
                        continue;
                    }

                    // Search the next level for the object.
                    var resultContainer = GetTreeViewItem(subContainer, item);
                    if (resultContainer != null)
                    {
                        return resultContainer;
                    }
                    
                }
            }

            return null;
        }

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = e.NewValue as TreeViewItem;
            if (item != null)
            {
                item.SetValue(TreeViewItem.IsSelectedProperty, true);
                return;
            }

            var behavior = (BindableSelectedItemBehavior)sender;
            var treeView = behavior.AssociatedObject;
            if (treeView == null)
            {
                // at designtime the AssociatedObject sometimes seems to be null
                return;
            }

            item = GetTreeViewItem(treeView, e.NewValue);
            if (item != null)
            {
                item.IsSelected = true;
            }
        }
        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
            {
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;
        }
    }
}
