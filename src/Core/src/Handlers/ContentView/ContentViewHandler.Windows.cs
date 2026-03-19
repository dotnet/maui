using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentViewHandler : ViewHandler<IContentView, ContentPanel>
	{
		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			PlatformView.CrossPlatformLayout = VirtualView;
		}

		static void UpdateContent(IContentViewHandler handler)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			handler.PlatformView.CachedChildren.Clear();

			if (handler.VirtualView.PresentedContent is IView view)
			{
				handler.PlatformView.CachedChildren.Add(view.ToPlatform(handler.MauiContext));
			}
		}

		protected override ContentPanel CreatePlatformView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
			}

			var view = new ContentPanel
			{
				CrossPlatformLayout = VirtualView,
				IsHitTestVisible = true
			};

			return view;
		}

		public static partial void MapContent(IContentViewHandler handler, IContentView page)
		{
			UpdateContent(handler);
		}

		protected override void DisconnectHandler(ContentPanel platformView)
		{
			platformView.CrossPlatformLayout = null;
			platformView.CachedChildren?.Clear();

			base.DisconnectHandler(platformView);
		}

		static UI.Xaml.FrameworkElement? CreateContent(IView content, IMauiContext mauiContext)
		{
			var platformContent = content.ToPlatform(mauiContext);

			var defaultBrush = new UI.Xaml.Media.SolidColorBrush(Colors.Transparent.ToWindowsColor());
			platformContent.UpdatePlatformViewBackground(content, defaultBrush);
			
			return platformContent;

		}
	}
}