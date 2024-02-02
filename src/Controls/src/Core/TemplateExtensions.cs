#nullable disable
using System;
using System.Linq.Expressions;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TemplateExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.TemplateExtensions']/Docs/*" />
	public static class TemplateExtensions
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/TemplateExtensions.xml" path="//Member[@MemberName='SetBinding']/Docs/*" />
		public static void SetBinding(this DataTemplate self, BindableProperty targetProperty, string path)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			self.SetBinding(targetProperty, new Binding(path));
		}

		internal static void SetBinding<TSource, TProperty>(this DataTemplate self, BindableProperty targetProperty, Expression<Func<TSource, TProperty>> getter, Action<TSource, TProperty> setter = null)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			self.SetBinding(targetProperty, TypedBindingFactory.Create<TSource, TProperty>(getter, setter));
		}
	}
}