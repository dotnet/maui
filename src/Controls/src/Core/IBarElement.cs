#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	interface IBarElement
	{
		Color BarBackgroundColor { get; }
		Brush BarBackground { get; }
		Color BarTextColor { get; }
		Color BarIndicatorColor { get; }
		BarTabMode BarTabMode { get; }
	}
}