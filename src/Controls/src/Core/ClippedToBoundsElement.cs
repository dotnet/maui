#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	static class ClippedToBoundsElement
	{
		public static readonly BindableProperty ClippedToBoundsProperty =
			BindableProperty.Create("ClippedToBounds", typeof(bool), typeof(IClippedToBoundsElement), false);
	}
}