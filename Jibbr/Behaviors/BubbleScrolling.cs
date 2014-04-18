using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Jibbr.Behaviors
{
    public static class BubbleScrolling
    {
        public static readonly DependencyProperty ScrollProperty
            = DependencyProperty.RegisterAttached("IsSendingMouseWheelEventToParent",
            typeof(bool),
            typeof(BubbleScrolling),
            new FrameworkPropertyMetadata(OnValueChanged));

        /// <summary>
        /// Gets the IsSendingMouseWheelEventToParent for a given <see cref="TextBox"/>.
        /// </summary>
        /// <param name="control">
        /// The <see cref="TextBox"/> whose IsSendingMouseWheelEventToParent is to be retrieved.
        /// </param>
        /// <returns>
        /// The IsSendingMouseWheelEventToParent, or <see langword="null"/>
        /// if no IsSendingMouseWheelEventToParent has been set.
        /// </returns>
        public static bool? GetIsSendingMouseWheelEventToParent(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("");

            return control.GetValue(ScrollProperty) as bool?;
        }

        /// <summary>
        /// Sets the IsSendingMouseWheelEventToParent for a given <see cref="TextBox"/>.
        /// </summary>
        /// <param name="control">
        /// The <see cref="TextBox"/> whose IsSendingMouseWheelEventToParent is to be set.
        /// </param>
        /// <param name="IsSendingMouseWheelEventToParent">
        /// The IsSendingMouseWheelEventToParent to set, or <see langword="null"/>
        /// to remove any existing IsSendingMouseWheelEventToParent from <paramref name="control"/>.
        /// </param>
        public static void SetIsSendingMouseWheelEventToParent(Control control, bool? sendToParent)
        {
            if (control == null)
                throw new ArgumentNullException("");

            control.SetValue(ScrollProperty, sendToParent);
        }

        private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = dependencyObject as Control;
            bool? IsSendingMouseWheelEventToParent = e.NewValue as bool?;
            scrollViewer.PreviewMouseWheel -= scrollViewer_PreviewMouseWheel;

            if (IsSendingMouseWheelEventToParent != null && IsSendingMouseWheelEventToParent != false)
            {
                scrollViewer.SetValue(ScrollProperty, IsSendingMouseWheelEventToParent);
                scrollViewer.PreviewMouseWheel += scrollViewer_PreviewMouseWheel;
            }
        }


        private static void scrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollview = sender as Control;

            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;
            if (scrollview.Parent == null)
                return;

            var parent = scrollview.Parent as UIElement;
            parent.RaiseEvent(eventArg);
        }
    }
}
