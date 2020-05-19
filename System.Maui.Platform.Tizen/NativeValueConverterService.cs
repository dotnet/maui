using System;
using System.Maui.Internals;
using System.Maui.Xaml.Internals;

using EObject = ElmSharp.EvasObject;

namespace System.Maui.Platform.Tizen
{
	class NativeValueConverterService : INativeValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object nativeValue)
		{
			nativeValue = null;
			if ((value is EObject) && toType.IsAssignableFrom(typeof(View)))
			{
				nativeValue = ((EObject)value).ToView();
				return true;
			}
			return false;
		}
	}
}
