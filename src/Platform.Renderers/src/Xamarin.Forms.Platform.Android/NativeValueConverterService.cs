using System;
using Xamarin.Forms.Xaml.Internals;
using AView = Android.Views.View;

[assembly: Xamarin.Forms.Dependency(typeof(Xamarin.Forms.Platform.Android.NativeValueConverterService))]
namespace Xamarin.Forms.Platform.Android
{
	class NativeValueConverterService : INativeValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object nativeValue)
		{
			nativeValue = null;
			if (typeof(AView).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View)))
			{
				nativeValue = ((AView)value).ToView();
				return true;
			}
			return false;
		}
	}
}