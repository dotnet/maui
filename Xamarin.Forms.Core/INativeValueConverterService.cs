using System;

namespace Xamarin.Forms.Xaml
{
	interface INativeValueConverterService
	{
		bool ConvertTo(object value, Type toType, out object nativeValue);
	}
}