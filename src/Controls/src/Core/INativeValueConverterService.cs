using System;

namespace Microsoft.Maui.Controls.Xaml.Internals
{
	public interface INativeValueConverterService
	{
		bool ConvertTo(object value, Type toType, out object nativeValue);
	}
}