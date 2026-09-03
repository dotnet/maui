using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Internals;
using ObjCRuntime;
using UIKit;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.iOS.NativeBindingService))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	class NativeBindingService : INativeBindingService
	{
		[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
		public bool TrySetBinding(object target, string propertyName, BindingBase binding)
		{
			Hosting.MauiAppBuilderExtensions.CheckForCompatibility();
			var view = target as UIView;
			if (view == null)
				return false;
			if (target.GetType().GetProperty(propertyName)?.GetMethod == null)
				return false;
			view.SetBinding(propertyName, binding);
			return true;
		}

		public bool TrySetBinding(object target, BindableProperty property, BindingBase binding)
		{
			Hosting.MauiAppBuilderExtensions.CheckForCompatibility();
			var view = target as UIView;
			if (view == null)
				return false;
			view.SetBinding(property, binding);
			return true;
		}

		public bool TrySetValue(object target, BindableProperty property, object value)
		{
			Hosting.MauiAppBuilderExtensions.CheckForCompatibility();
			var view = target as UIView;
			if (view == null)
				return false;
			view.SetValue(property, value);
			return true;
		}
	}
}