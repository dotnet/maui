using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	public static class DataTemplateExtensions
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static object CreateContent(this DataTemplate self, object item, BindableObject container)
		{
			var selector = self as DataTemplateSelector;
			if (selector != null)
			{
				self = selector.SelectTemplate(item, container);
			}
			return self.CreateContent();
		}
	}
}