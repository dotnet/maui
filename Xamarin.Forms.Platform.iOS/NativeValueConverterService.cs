using System;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(Xamarin.Forms.Platform.iOS.NativeValueConverterService))]

namespace Xamarin.Forms.Platform.iOS
{
	class NativeValueConverterService : Xaml.INativeValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object nativeValue)
		{
			nativeValue = null;
			if (typeof(UIView).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View))) {
				nativeValue = ((UIView)value).ToView();
				return true;
			}
			return false;
		}
	}
}