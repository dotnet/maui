using System;
using System.Collections.Generic;
using Xamarin.Forms.Internals;
using EObject = ElmSharp.EvasObject;

namespace Xamarin.Forms.Platform.Tizen
{
	public static class NativeBindingExtensions
	{
		public static void SetBinding(this EObject view, string propertyName, BindingBase binding, string updateSourceEventName = null)
		{
			NativeBindingHelpers.SetBinding(view, propertyName, binding, updateSourceEventName);
		}

		public static void SetBinding(this EObject view, BindableProperty targetProperty, BindingBase binding)
		{
			NativeBindingHelpers.SetBinding(view, targetProperty, binding);
		}

		public static void SetValue(this EObject target, BindableProperty targetProperty, object value)
		{
			NativeBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this EObject target, object bindingContext, Func<EObject, IEnumerable<EObject>> getChildren = null)
		{
			NativeBindingHelpers.SetBindingContext(target, bindingContext, getChildren);
		}

		internal static void TransferBindablePropertiesToWrapper(this EObject target, View wrapper)
		{
			NativeBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}
	}
}
