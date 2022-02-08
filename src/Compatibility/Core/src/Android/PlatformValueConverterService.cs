using System;
using Microsoft.Maui.Controls.Xaml.Internals;
using AView = Android.Views.View;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.Android.PlatformValueConverterService))]
namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	class PlatformValueConverterService : IPlatformValueConverterService
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