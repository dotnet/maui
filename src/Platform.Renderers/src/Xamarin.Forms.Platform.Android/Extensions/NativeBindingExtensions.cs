using System;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	public static class NativeBindingExtensions
	{
		public static void SetBinding(this global::Android.Views.View view, string propertyName, BindingBase binding, string updateSourceEventName = null)
		{
			NativeBindingHelpers.SetBinding(view, propertyName, binding, updateSourceEventName);
		}

		public static void SetBinding(this global::Android.Views.View view, BindableProperty targetProperty, BindingBase binding)
		{
			NativeBindingHelpers.SetBinding(view, targetProperty, binding);
		}

		public static void SetValue(this global::Android.Views.View target, BindableProperty targetProperty, object value)
		{
			NativeBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this global::Android.Views.View target, object bindingContext, Func<global::Android.Views.View, IEnumerable<global::Android.Views.View>> getChildren = null)
		{
			NativeBindingHelpers.SetBindingContext(target, bindingContext, getChildren);
		}

		internal static void TransferBindablePropertiesToWrapper(this global::Android.Views.View target, View wrapper)
		{
			NativeBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}
	}
}