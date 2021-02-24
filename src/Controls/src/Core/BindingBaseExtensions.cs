using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls
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