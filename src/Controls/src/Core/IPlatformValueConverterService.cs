using System;

namespace Microsoft.Maui.Controls.Xaml.Internals
{
	public interface IPlatformValueConverterService
	{
		bool ConvertTo(object value, Type toType, out object platformValue);
	}
}