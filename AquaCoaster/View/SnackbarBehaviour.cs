using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using WPFUI.Controls;

namespace AquaCoaster.View
{
    public static class SnackbarBehaviour
    {
        public static bool GetExpandTrigger(Snackbar target)
        {
            return (bool)target.GetValue(ExpandTriggerAttachedProperty);
        }

        public static void SetExpandTrigger(Snackbar target, bool value)
        {
            target.SetValue(ExpandTriggerAttachedProperty, value);
        }

        public static readonly DependencyProperty ExpandTriggerAttachedProperty = DependencyProperty.RegisterAttached("ExpandTrigger", typeof(bool), typeof(SnackbarBehaviour), new UIPropertyMetadata(false, OnExpandTriggerPropertyChanged));

        static void OnExpandTriggerPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
           ((Snackbar)o).Expand();
        }
    }
}
