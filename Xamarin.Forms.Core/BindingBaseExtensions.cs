using System;

namespace Xamarin.Forms
{
	internal static class BindingBaseExtensions
	{
		internal static BindingMode GetRealizedMode(this BindingBase self, BindableProperty property)
		{
			if (self == null)
				throw new ArgumentNullException("self");
			if (property == null)
				throw new ArgumentNullException("property");

			return self.Mode != BindingMode.Default ? self.Mode : property.DefaultBindingMode;
		}
	}
}