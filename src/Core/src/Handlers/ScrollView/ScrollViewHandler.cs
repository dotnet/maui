#nullable enable
using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler
	{
		public static PropertyMapper<IScrollView, ScrollViewHandler> ScrollViewMapper = new PropertyMapper<IScrollView, ScrollViewHandler>(ViewMapper)
		{
			[nameof(IScrollView.Content)] = MapContent,
			[nameof(IScrollView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
			[nameof(IScrollView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
			[nameof(IScrollView.Orientation)] = MapOrientation
		};

		// TODO ezhart This should chain in ViewHandler.CommandMapper
		public static CommandMapper<IScrollView, ScrollViewHandler> ScrollViewCommandMapper = new CommandMapper<IScrollView, ScrollViewHandler>() {
			[nameof(IScrollView.RequestScrollTo)] = MapRequestScrollTo
		};

		public ScrollViewHandler() : base(ScrollViewMapper, ScrollViewCommandMapper)
		{

		}

		public ScrollViewHandler(PropertyMapper? mapper = null) : base(mapper ?? ScrollViewMapper)
		{

		}
	}
}
