using System.Runtime.CompilerServices;

namespace Xamarin.Forms
{
	static class BindingBaseExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BindingMode GetRealizedMode(this BindingBase self, BindableProperty property)
		{
			return self.Mode != BindingMode.Default ? self.Mode : property.DefaultBindingMode;
		}
	}
}