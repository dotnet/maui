using System;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.Maui.Controls.Xaml.Internals;


[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.UWP.NativeValueConverterService))]
namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
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
