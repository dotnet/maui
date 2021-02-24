using System;

namespace Microsoft.Maui.Controls.Xaml
{
	public interface IValueProvider
	{
		object ProvideValue(IServiceProvider serviceProvider);
	}
}