using System;

namespace System.Maui.Xaml.Internals
{
	public interface INativeValueConverterService
	{
		bool ConvertTo(object value, Type toType, out object nativeValue);
	}
}