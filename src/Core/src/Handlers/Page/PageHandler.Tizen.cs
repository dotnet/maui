using Tizen.UIExtensions.Common;
using EColor = ElmSharp.Color;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ContentViewHandler
	{
		public static void MapBackground(IPageHandler handler, IContentView page)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			if (page.Background != null && handler.PlatformView.BackgroundColor != EColor.Transparent)
			{
				handler.PlatformView.BackgroundColor = EColor.Transparent;
			}
			handler.ToPlatform()?.UpdateBackground(page);
		}

		[MissingMapper]
		public static void MapTitle(IPageHandler handler, IContentView page) { }

		protected override ContentCanvas CreatePlatformView()
		{
			var view = base.CreatePlatformView();
			view.BackgroundColor = (DeviceInfo.GetDeviceType() == DeviceType.TV) ? EColor.Transparent : EColor.White;

			return view;
		}
	}
}