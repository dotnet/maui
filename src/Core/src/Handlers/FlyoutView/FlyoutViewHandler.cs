using System;

namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler
	{
		public static IPropertyMapper<IFlyoutView, FlyoutViewHandler> Mapper = new PropertyMapper<IFlyoutView, FlyoutViewHandler>(ViewHandler.ViewMapper)
		{
#if ANDROID || WINDOWS
			[nameof(IFlyoutView.Flyout)] = MapFlyout,
			[nameof(IFlyoutView.Detail)] = MapDetail,
			[nameof(IFlyoutView.IsPresented)] = MapIsPresented,
			[nameof(IFlyoutView.FlyoutBehavior)] = MapFlyoutBehavior,
			[nameof(IFlyoutView.FlyoutWidth)] = MapFlyoutWidth,
			[nameof(IFlyoutView.IsGestureEnabled)] = MapIsGestureEnabled,
			[nameof(IToolbarElement.Toolbar)] = MapToolbar,
#endif
		};

		public FlyoutViewHandler() : base(Mapper)
		{
		}
	}
}
