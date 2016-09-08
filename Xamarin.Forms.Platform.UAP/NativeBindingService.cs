using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

#if WINDOWS_UWP
[assembly: Xamarin.Forms.Dependency(typeof(Xamarin.Forms.Platform.UWP.NativeBindingService))]
namespace Xamarin.Forms.Platform.UWP
#else
[assembly: Xamarin.Forms.Dependency(typeof(Xamarin.Forms.Platform.WinRT.NativeBindingService))]
namespace Xamarin.Forms.Platform.WinRT
#endif
{
    public class NativeBindingService : Xaml.INativeBindingService
    {
        public bool TrySetBinding(object target, string propertyName, BindingBase binding)
        {
            var view = target as FrameworkElement;
            if (view == null)
                return false;
            if (target.GetType().GetProperty(propertyName)?.GetMethod == null)
                return false;
            view.SetBinding(propertyName, binding);
            return true;
        }

        public bool TrySetBinding(object target, BindableProperty property, BindingBase binding)
        {
            var view = target as FrameworkElement;
            if (view == null)
                return false;
            view.SetBinding(property, binding);
            return true;
        }

        public bool TrySetValue(object target, BindableProperty property, object value)
        {
            var view = target as FrameworkElement;
            if (view == null)
                return false;
            view.SetValue(property, value);
            return true;
        }
    }
}
