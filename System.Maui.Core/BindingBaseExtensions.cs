using System.Runtime.CompilerServices;

namespace System.Maui
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