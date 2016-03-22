using System;

namespace Xamarin.Forms.Xaml
{
	public class NullExtension : IMarkupExtension
	{
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return null;
		}
	}
}