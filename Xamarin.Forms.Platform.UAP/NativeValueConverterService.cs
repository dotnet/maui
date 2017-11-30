using System;
using System.Reflection;
using Windows.UI.Xaml;
using Xamarin.Forms.Xaml.Internals;


[assembly: Xamarin.Forms.Dependency(typeof(Xamarin.Forms.Platform.UWP.NativeValueConverterService))]
namespace Xamarin.Forms.Platform.UWP
{
    public class NativeValueConverterService : INativeValueConverterService
    {
        public bool ConvertTo(object value, Type toType, out object nativeValue)
        {
            nativeValue = null;
            if (typeof(FrameworkElement).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View)))
            {
                nativeValue = ((FrameworkElement)value).ToView();
                return true;
            }
            return false;
        }
    }
}
