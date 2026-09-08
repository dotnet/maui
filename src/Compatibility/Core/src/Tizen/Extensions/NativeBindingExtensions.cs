using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Internals;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public static class NativeBindingExtensions
	{
		[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
		public static void SetBinding(this NView view, string propertyName, BindingBase binding, string updateSourceEventName = null)
		{
			PlatformBindingHelpers.SetBinding(view, propertyName, binding, updateSourceEventName);
		}

		public static void SetBinding(this NView view, BindableProperty targetProperty, BindingBase binding)
		{
			PlatformBindingHelpers.SetBinding(view, targetProperty, binding);
		}

		public static void SetValue(this NView target, BindableProperty targetProperty, object value)
		{
			PlatformBindingHelpers.SetValue(target, targetProperty, value);
		}

		public static void SetBindingContext(this NView target, object bindingContext, Func<NView, IEnumerable<NView>> getChildren = null)
		{
			PlatformBindingHelpers.SetBindingContext(target, bindingContext, getChildren);
		}

		internal static void TransferBindablePropertiesToWrapper(this NView target, View wrapper)
		{
			PlatformBindingHelpers.TransferBindablePropertiesToWrapper(target, wrapper);
		}
	}
}
