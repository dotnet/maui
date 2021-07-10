#nullable enable
using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler
	{
		public static PropertyMapper<IScrollView, ScrollViewHandler> ScrollViewMapper = new PropertyMapper<IScrollView, ScrollViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IScrollView.Content)] = MapContent,
			[nameof(IScrollView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
			[nameof(IScrollView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
			[nameof(IScrollView.Orientation)] = MapOrientation,
			Actions =
			{
				[nameof(IScrollView.RequestScrollTo)] = MapRequestScrollTo
			}
		};

		private static void MapRequestScrollTo(ScrollViewHandler arg1, IScrollView arg2)
		{
			throw new NotImplementedException();
		}

		public ScrollViewHandler() : base(ScrollViewMapper)
		{

		}

		public ScrollViewHandler(PropertyMapper? mapper = null) : base(mapper ?? ScrollViewMapper)
		{

		}
	}
}
