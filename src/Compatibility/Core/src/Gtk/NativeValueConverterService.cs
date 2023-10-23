using System;
using Microsoft.Maui.Controls.Xaml.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.Gtk.NativeValueConverterService))]
namespace Microsoft.Maui.Controls.Compatibility.Platform.Gtk
{
	class NativeValueConverterService : INativeValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object nativeValue)
		{
			nativeValue = null;
			if (typeof(global::Gtk.Widget).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View)))
			{
				nativeValue = ((global::Gtk.Widget)value);
				return true;
			}
			return false;
		}
	}
}