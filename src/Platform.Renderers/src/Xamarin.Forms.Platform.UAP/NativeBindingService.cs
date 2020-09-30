using System.Reflection;
using Windows.UI.Xaml;
using Xamarin.Forms.Xaml.Internals;


[assembly: Xamarin.Forms.Dependency(typeof(Xamarin.Forms.Platform.UWP.NativeBindingService))]
namespace Xamarin.Forms.Platform.UWP
{
    public class NativeBindingService : INativeBindingService
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
