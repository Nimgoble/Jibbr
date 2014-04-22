using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

using Jibbr.Extensions;

namespace Jibbr.Behaviors
{
    public class AutoScroller : Behavior<UIElement>
    {
        #region Private Members
        private ItemsControl scrollableControl = null;
        private INotifyCollectionChanged targetCollection = null;
        private DependencyPropertyDescriptor itemsSourcePropertyDescriptor = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl));
        #endregion

        protected override void OnAttached()
        {
            scrollableControl = AssociatedObject as ItemsControl;

            if (itemsSourcePropertyDescriptor != null)
                itemsSourcePropertyDescriptor.AddValueChanged(scrollableControl, ItemsSourceBound);

            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            if (itemsSourcePropertyDescriptor != null)
                itemsSourcePropertyDescriptor.RemoveValueChanged(scrollableControl, ItemsSourceBound);

            //If we have a target collection, remove the callback
            if (targetCollection != null)
                targetCollection.CollectionChanged -= itemsSource_CollectionChanged;

            base.OnDetaching();
        }
        /// <summary>
        /// Called when the ItemsSource for our target control is bound to something
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemsSourceBound(object sender, EventArgs e)
        {
            if (scrollableControl == null)
                return;

            //If we already have a target collection, remove the callback
            if (targetCollection != null)
                targetCollection.CollectionChanged -= itemsSource_CollectionChanged;

            //Get the new target collection
            targetCollection = scrollableControl.ItemsSource as INotifyCollectionChanged;
            if (targetCollection == null)
                return;
            //Add the callback
            targetCollection.CollectionChanged += itemsSource_CollectionChanged;
        }
        /// <summary>
        /// Called when the ItemsSource collection is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void itemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (scrollableControl == null)
                return;

            var scrollViewer = scrollableControl.GetDescendantByType<ScrollViewer>();
            if (scrollViewer != null)
                scrollViewer.ScrollToEnd();
        }
    }
}
