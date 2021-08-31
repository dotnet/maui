using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Gtk
{
	public static class NativeBindingExtensions
	{
		public static void SetBinding(this global::Gtk.Widget view, string propertyName, BindingBase binding, string updateSourceEventName = null)
		{
			NativeBindingHelpers.SetBinding(view, propertyName, binding, updateSourceEventName);
		}

		public static void SetBinding(this global::Gtk.Widget view, BindableProperty targetProperty, BindingBase binding)
		{
			NativeBindingHelpers.SetBinding(view, targetProperty, binding);
		}

		public static void SetValue(this global::Gtk.Widget target, BindableProperty targetProperty, object value)
		{
			NativeBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this global::Gtk.Widget target, object bindingContext, Func<global::Gtk.Widget, IEnumerable<global::Gtk.Widget>> getChildren = null)
		{
			NativeBindingHelpers.SetBindingContext(target, bindingContext, getChildren);
		}

		internal static void TransferBindablePropertiesToWrapper(this global::Gtk.Widget target, View wrapper)
		{
			NativeBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}
	}
}