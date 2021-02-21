using System;

namespace Xamarin.Forms
{
	public interface IExtendedTypeConverter
	{
		object ConvertFromInvariantString(string value, IServiceProvider serviceProvider);
	}
}