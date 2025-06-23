#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Xaml.Internals
{

	[Obsolete("INativeBindingService is obsolete and will be removed in a future release.")]
	public interface INativeBindingService
	{
		[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
		bool TrySetBinding(object target, string propertyName, BindingBase binding);
		bool TrySetBinding(object target, BindableProperty property, BindingBase binding);
		bool TrySetValue(object target, BindableProperty property, object value);
	}
}