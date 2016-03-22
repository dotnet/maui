using System;

namespace Xamarin.Forms
{
	public static class TemplateExtensions
	{
		public static void SetBinding(this DataTemplate self, BindableProperty targetProperty, string path)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			self.SetBinding(targetProperty, new Binding(path));
		}
	}
}