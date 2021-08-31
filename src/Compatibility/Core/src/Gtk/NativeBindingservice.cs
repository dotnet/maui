using System;
using Microsoft.Maui.Controls.Xaml.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.Gtk.NativeBindingService))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.Gtk
{
	class NativeBindingService : INativeBindingService
	{
		public bool TrySetBinding(object target, string propertyName, BindingBase binding)
		{
			var view = target as global::Gtk.Widget;
			if (view == null)
				return false;
			if (target.GetType().GetProperty(propertyName)?.GetMethod == null)
				return false;
			view.SetBinding(propertyName, binding);
			return true;
		}

		public bool TrySetBinding(object target, BindableProperty property, BindingBase binding)
		{
			var view = target as global::Gtk.Widget;
			if (view == null)
				return false;
			view.SetBinding(property, binding);
			return true;
		}

		public bool TrySetValue(object target, BindableProperty property, object value)
		{
			var view = target as global::Gtk.Widget;
			if (view == null)
				return false;
			view.SetValue(property, value);
			return true;
		}
	}
}