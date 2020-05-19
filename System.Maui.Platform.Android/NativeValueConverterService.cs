using System;
using System.Maui.Xaml.Internals;
using AView = Android.Views.View;

[assembly: System.Maui.Dependency(typeof(System.Maui.Platform.Android.NativeValueConverterService))]
namespace System.Maui.Platform.Android
{
	class NativeValueConverterService : INativeValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object nativeValue)
		{
			nativeValue = null;
			if (typeof(AView).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View))) {
				nativeValue = ((AView)value).ToView();
				return true;
			}
			return false;
		}
	}
}