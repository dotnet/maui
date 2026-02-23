#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <summary>Provides extension methods for working with data templates.</summary>
	public static class TemplateExtensions
	{
		/// <summary>Creates a binding on the template for the specified property and path.</summary>
		/// <param name="self">The data template to add the binding to.</param>
		/// <param name="targetProperty">The target bindable property.</param>
		/// <param name="path">The binding path expression.</param>
		[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
		public static void SetBinding(this DataTemplate self, BindableProperty targetProperty, string path)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			self.SetBinding(targetProperty, new Binding(path));
		}
	}
}