#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Extension methods for <see cref="DataTemplate"/> that support template selection.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class DataTemplateExtensions
	{
		/// <summary>Returns the appropriate template, invoking selector logic if the template is a <see cref="DataTemplateSelector"/>.</summary>
		/// <param name="self">The template or selector.</param>
		/// <param name="item">The data item.</param>
		/// <param name="container">The container that will display the item.</param>
		/// <returns>The selected <see cref="DataTemplate"/>.</returns>
		public static DataTemplate SelectDataTemplate(this DataTemplate self, object item, BindableObject container)
		{
			var selector = self as DataTemplateSelector;
			if (selector == null)
				return self;

			return selector.SelectTemplate(item, container);
		}

		/// <summary>Selects the appropriate template and creates its content for the specified item.</summary>
		/// <param name="self">The template or selector.</param>
		/// <param name="item">The data item.</param>
		/// <param name="container">The container that will display the item.</param>
		/// <returns>The created content object.</returns>
		public static object CreateContent(this DataTemplate self, object item, BindableObject container)
		{
			return self.SelectDataTemplate(item, container).CreateContent();
		}
	}
}