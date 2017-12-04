using System;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Internals;

using EObject = ElmSharp.EvasObject;

namespace Xamarin.Forms.Platform.Tizen
{
	class NativeValueConverterService : INativeValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object nativeValue)
		{
			nativeValue = null;
			if (typeof(EObject).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View)))
			{
				nativeValue = ((EObject)value).ToView();
				return true;
			}
			return false;
		}
	}
}
