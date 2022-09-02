using Tizen.UIExtensions.NUI;
using TColor = Tizen.UIExtensions.Common.Color;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ContentViewHandler
	{
		public static void MapBackground(IPageHandler handler, IContentView page)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			if (page.Background != null && handler.PlatformView.BackgroundColor != Tizen.NUI.Color.Transparent)
			{
				handler.PlatformView.UpdateBackgroundColor(TColor.Transparent);
			}
			handler.ToPlatform()?.UpdateBackground(page);
		}

		public override void PlatformArrange(Graphics.Rect frame)
		{
			// empty on purpose
		}

		[MissingMapper]
		public static void MapTitle(IPageHandler handler, IContentView page) { }

		protected override ContentViewGroup CreatePlatformView()
		{
			var view = base.CreatePlatformView();
			view.UpdateBackgroundColor(TColor.White);

			return view;
		}
	}
}