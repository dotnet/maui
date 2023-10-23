using System;

namespace Microsoft.Maui.Handlers
{

	public partial class ContentViewHandler : ViewHandler<IContentView, ContentView>
	{

		protected override ContentView CreatePlatformView() => new();

		public void UpdateContent()
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (VirtualView is { Content: IView view })
				PlatformView.Content = view.ToPlatform(MauiContext);
		}

		public static void MapContent(IContentViewHandler handler, IContentView page)
		{
			if (handler is ContentViewHandler contentViewHandler)
			{
				contentViewHandler.UpdateContent();
			}
		}

	}

}