using System;
using System.Reflection;
using global::Windows.UI.Xaml;
using System.Maui.Xaml.Internals;


[assembly: System.Maui.Dependency(typeof(System.Maui.Platform.UWP.NativeValueConverterService))]
namespace System.Maui.Platform.UWP
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
