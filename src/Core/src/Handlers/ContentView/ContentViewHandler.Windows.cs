using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;

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

			if (handler.VirtualView.PresentedContent is IView view)
			{
				var platformView = view.ToPlatform(handler.MauiContext);

				// Detach from existing parent — mirrors Android RemoveFromParent / iOS RemoveFromSuperview.
				// When the parent is a ContentPanel, use its Content setter so the internal _content
				// field is cleared consistently (clip/border logic relies on ContentPanel._content).
				if (platformView is FrameworkElement fwElement && fwElement.Parent is not null)
				{
					if (fwElement.Parent is ContentPanel existingContentPanel)
					{
						existingContentPanel.Content = null;
					}
					else if (fwElement.Parent is MauiPanel existingPanel)
					{
						existingPanel.CachedChildren.Remove(fwElement);
					}
				}

				handler.PlatformView.Content = platformView;
			}
			else
			{
				handler.PlatformView.Content = null;
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