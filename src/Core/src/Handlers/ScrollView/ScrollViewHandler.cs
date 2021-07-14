#nullable enable
using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler
	{
		public static PropertyMapper<IScrollView, ScrollViewHandler> ScrollViewMapper = new(ViewMapper)
		{
			[nameof(IScrollView.Content)] = MapContent,
			[nameof(IScrollView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
			[nameof(IScrollView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
			[nameof(IScrollView.Orientation)] = MapOrientation,
		};

		public ScrollViewHandler() : base(ScrollViewMapper)
		{

		}
	}
}