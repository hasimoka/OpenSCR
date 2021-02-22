using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace OpenSCR
{
    public class DataContextDisposeAction : TriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
            => (this.AssociatedObject?.DataContext as IDisposable)?.Dispose();
    }
}
