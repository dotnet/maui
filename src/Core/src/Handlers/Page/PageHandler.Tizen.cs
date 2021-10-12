using EColor = ElmSharp.Color;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ContentViewHandler
	{
		public static void MapBackground(PageHandler handler, IContentView page)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			if (page.Background != null && handler.NativeView.BackgroundColor != EColor.Transparent)
			{
				handler.NativeView.BackgroundColor = EColor.Transparent;
			}
			handler.GetWrappedNativeView()?.UpdateBackground(page);
		}

		public static void MapTitle(PageHandler handler, IContentView page)
		{
			var view = base.CreateNativeView();
			view.BackgroundColor = (DeviceInfo.GetDeviceType() == DeviceType.TV) ? EColor.Transparent : EColor.White;

			return view;
		}
	}
}