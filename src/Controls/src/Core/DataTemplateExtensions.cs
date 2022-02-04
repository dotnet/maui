using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../docs/Microsoft.Maui.Controls.Internals/DataTemplateExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.DataTemplateExtensions']/Docs" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class DataTemplateExtensions
	{
		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/DataTemplateExtensions.xml" path="//Member[@MemberName='SelectDataTemplate']/Docs" />
		public static DataTemplate SelectDataTemplate(this DataTemplate self, object item, BindableObject container)
		{
			var selector = self as DataTemplateSelector;
			if (selector == null)
				return self;

			return selector.SelectTemplate(item, container);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/DataTemplateExtensions.xml" path="//Member[@MemberName='CreateContent']/Docs" />
		public static object CreateContent(this DataTemplate self, object item, BindableObject container)
		{
			return self.SelectDataTemplate(item, container).CreateContent();
		}
	}
}