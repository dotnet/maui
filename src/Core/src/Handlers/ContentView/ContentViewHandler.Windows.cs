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
				if (!ReferenceEquals(handler.VirtualView.Content, view))
				{
					handler.PlatformView.Content = null;
				}
				
				// Detach from existing parent — mirrors Android RemoveFromParent / iOS RemoveFromSuperview.
				// Always remove via CachedChildren directly: Content = null is a no-op when _content
				// is null (e.g. ScrollViewHandler adds via paddingShim.CachedChildren.Add, not the
				// Content setter), leaving the element with a live parent and causing a COM exception
				// when we try to reparent it. Only clear _content when it actually tracks fwElement.
				var platformView = view.ToPlatform(handler.MauiContext);
				RemoveFromParent(platformView);

				handler.PlatformView.CachedChildren.Clear();
				handler.PlatformView.Content = platformView;	
			}
			else
			{
				handler.PlatformView.Content = null;
				handler.PlatformView.CachedChildren.Clear();
			}
		}

		static void RemoveFromParent(FrameworkElement? platformView)
		{
			if (platformView?.Parent is ContentPanel existingContentPanel)
			{
				existingContentPanel.CachedChildren.Remove(platformView);
				if (existingContentPanel.Content == platformView)
				{
					existingContentPanel.Content = null;
				}
			}
			else if (platformView?.Parent is MauiPanel existingPanel)
			{
				existingPanel.CachedChildren.Remove(platformView);
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