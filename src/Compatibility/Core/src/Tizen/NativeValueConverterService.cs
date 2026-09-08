using System;
using Microsoft.Maui.Controls.Xaml.Internals;

using NUI = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	class NativeValueConverterService : INativeValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object nativeValue)
		{
			Hosting.MauiAppBuilderExtensions.CheckForCompatibility();
			nativeValue = null;
			if ((value is NUI) && toType.IsAssignableFrom(typeof(View)))
			{
				nativeValue = ((NUI)value).ToView();
				return true;
			}
			return false;
		}
	}
}
