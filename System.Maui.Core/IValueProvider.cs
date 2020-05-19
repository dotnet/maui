using System;

namespace System.Maui.Xaml
{
	public interface IValueProvider
	{
		object ProvideValue(IServiceProvider serviceProvider);
	}
}