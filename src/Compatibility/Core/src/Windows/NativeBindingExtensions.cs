using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.Maui.Controls.Internals;
using static System.String;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public static class NativeBindingExtensions
	{
		public static void SetBinding(this FrameworkElement view, string propertyName, BindingBase bindingBase, string updateSourceEventName = null)
		{
			var binding = bindingBase as Binding;
			updateSourceEventName = updateSourceEventName ?? binding?.UpdateSourceEventName;

			if (IsNullOrEmpty(updateSourceEventName))
			{
				NativePropertyListener nativePropertyListener = null;
				if (bindingBase.Mode == BindingMode.TwoWay)
					nativePropertyListener = new NativePropertyListener(view, propertyName);

				NativeBindingHelpers.SetBinding(view, propertyName, bindingBase, nativePropertyListener as INotifyPropertyChanged);
				return;
			}

			NativeEventWrapper eventE = null;
			if (binding.Mode == BindingMode.TwoWay && !(view is INotifyPropertyChanged))
				eventE = new NativeEventWrapper(view, propertyName, updateSourceEventName);

			NativeBindingHelpers.SetBinding(view, propertyName, binding, eventE);
		}

		public static void SetBinding(this FrameworkElement view, BindableProperty targetProperty, BindingBase binding)
		{
			NativeBindingHelpers.SetBinding(view, targetProperty, binding);
		}

		public static void SetValue(this FrameworkElement target, BindableProperty targetProperty, object value)
		{
			NativeBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this FrameworkElement target, object bindingContext, Func<FrameworkElement, IEnumerable<FrameworkElement>> getChildren = null)
		{
			NativeBindingHelpers.SetBindingContext(target, bindingContext, getChildren);
		}

		internal static void TransferbindablePropertiesToWrapper(this FrameworkElement target, View wrapper)
		{
			NativeBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}
	}
}