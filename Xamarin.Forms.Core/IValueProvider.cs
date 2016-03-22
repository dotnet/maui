using System;

namespace Xamarin.Forms.Xaml
{
	public interface IValueProvider
	{
		object ProvideValue(IServiceProvider serviceProvider);
	}
}