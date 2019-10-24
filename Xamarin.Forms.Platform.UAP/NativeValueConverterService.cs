using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Internals;

#if WINDOWS_UWP
[assembly: Xamarin.Forms.Dependency(typeof(Xamarin.Forms.Platform.UWP.NativeValueConverterService))]
namespace Xamarin.Forms.Platform.UWP
#else
[assembly: Xamarin.Forms.Dependency(typeof(Xamarin.Forms.Platform.WinRT.NativeValueConverterService))]
namespace Xamarin.Forms.Platform.WinRT
#endif
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
