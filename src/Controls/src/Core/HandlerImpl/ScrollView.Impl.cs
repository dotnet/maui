using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class ScrollView : IScroll
	{
		IView IScroll.Content { get; }

		SizeF IScroll.ContentSize { get; }
	}
}