using System;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.Maui.Controls.Xaml.Internals;


[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.UWP.PlatformValueConverterService))]
namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
    public class PlatformValueConverterService : IPlatformValueConverterService
    {
        public bool ConvertTo(object value, Type toType, out object platformValue)
        {
            platformValue = null;
            if (typeof(FrameworkElement).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View)))
            {
                platformValue = ((FrameworkElement)value).ToView();
                return true;
            }
            return false;
        }
    }
}
