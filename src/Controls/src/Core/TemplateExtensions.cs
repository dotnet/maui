#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TemplateExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.TemplateExtensions']/Docs/*" />
	public static class TemplateExtensions
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/TemplateExtensions.xml" path="//Member[@MemberName='SetBinding']/Docs/*" />
		[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
		public static void SetBinding(this DataTemplate self, BindableProperty targetProperty, string path)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			self.SetBinding(targetProperty, new Binding(path));
		}
	}
}