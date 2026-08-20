using System;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : ViewHandler<IBorderView, ContentPanel>
	{
		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			PlatformView.EnableContentClip();
			PlatformView.UpdateCrossPlatformLayout(VirtualView);
		}

		static partial void UpdateContent(IBorderHandler handler)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			handler.PlatformView.EnsureBorderPath();

			if (handler.VirtualView.PresentedContent is IView view)
			{
				var platformView = view.ToPlatform(handler.MauiContext);
				ReparentContent(handler.PlatformView, platformView);
			}
			else
			{
				handler.PlatformView.Content = null;
			}

		}

		internal static void ReparentContent(ContentPanel target, FrameworkElement platformView)
		{
			var parent = platformView.Parent;
			var isCurrentContent =
				ReferenceEquals(target.Content, platformView) &&
				(ReferenceEquals(parent, target) || ReferenceEquals(parent, target.ContentClipHost));

			if (isCurrentContent)
			{
				return;
			}

			// Detach from existing parent — mirrors Android RemoveFromParent / iOS RemoveFromSuperview.
			// Always remove via CachedChildren directly: Content = null is a no-op when _content
			// is null (e.g. ScrollViewHandler adds via paddingShim.CachedChildren.Add, not the
			// Content setter), leaving the element with a live parent and causing a COM exception
			// when we try to reparent it.
			if (parent is ContentPanelClipHost existingClipHost)
			{
				if (ReferenceEquals(existingClipHost.Owner.Content, platformView))
				{
					existingClipHost.Owner.Content = null;
				}
				else
				{
					existingClipHost.CachedChildren.Remove(platformView);
				}
			}
			else if (parent is ContentPanel existingContentPanel)
			{
				if (ReferenceEquals(existingContentPanel.Content, platformView))
				{
					existingContentPanel.Content = null;
				}
				else
				{
					existingContentPanel.CachedChildren.Remove(platformView);
				}
			}
			else if (parent is MauiPanel existingPanel)
			{
				existingPanel.CachedChildren.Remove(platformView);
			}

			if (ReferenceEquals(target.Content, platformView))
			{
				target.Content = null;
			}

			target.Content = platformView;
		}

		internal static void MapInvalidateMeasure(BorderHandler handler, IBorderView view, object? args)
		{
			ViewHandler.ViewCommandMapper.GetCommand(nameof(IView.InvalidateMeasure))?.Invoke(handler, view, args);

			if (((IElementHandler)handler).PlatformView is not ContentPanel platformView)
			{
				return;
			}

			// The clip host owns cross-platform layout, so it must be invalidated with the outer panel.
			platformView.ContentClipHost?.InvalidateMeasure();
		}

		protected override ContentPanel CreatePlatformView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
			}

			var view = new ContentPanel
			{
				CrossPlatformLayout = VirtualView
			};
			view.EnableContentClip();

			return view;
		}

		private protected override void OnDisconnectHandler(Microsoft.UI.Xaml.FrameworkElement platformView)
		{
			try
			{
				base.OnDisconnectHandler(platformView);
			}
			finally
			{
				((ContentPanel)platformView).DisconnectContent();
			}
		}

	}
}
