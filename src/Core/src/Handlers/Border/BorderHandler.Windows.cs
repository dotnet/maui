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

			PlatformView.CrossPlatformLayout = VirtualView;
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

				// Detach from existing parent — mirrors Android RemoveFromParent / iOS RemoveFromSuperview.
				// Always remove via CachedChildren directly: Content = null is a no-op when _content
				// is null (e.g. ScrollViewHandler adds via paddingShim.CachedChildren.Add, not the
				// Content setter), leaving the element with a live parent and causing a COM exception
				// when we try to reparent it. Only clear _content when it actually tracks fwElement.
				if (platformView is FrameworkElement fwElement && fwElement.Parent is not null)
				{
					if (fwElement.Parent is ContentPanel existingContentPanel)
					{
						existingContentPanel.CachedChildren.Remove(fwElement);
						if (existingContentPanel.Content == fwElement)
						{
							existingContentPanel.Content = null;
						}
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
				CrossPlatformLayout = VirtualView
			};

			return view;
		}
	}
}
