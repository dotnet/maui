#nullable disable
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls
{
	static class BindingBaseExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BindingMode GetRealizedMode(this BindingBase self, BindableProperty property)
		{
			var mode = self.Mode != BindingMode.Default ? self.Mode : property.DefaultBindingMode;

			if (mode == BindingMode.TwoWay && property.IsReadOnly)
				return BindingMode.OneWayToSource;

			return mode;
		}
	}
}