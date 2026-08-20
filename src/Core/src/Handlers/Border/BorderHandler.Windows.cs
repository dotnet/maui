using System;

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

			handler.PlatformView.Content = null;
			handler.PlatformView.EnsureBorderPath();

			if (handler.VirtualView.PresentedContent is IView view)
			{
				// Detach the old handler if it exists (prevents WinUI COM exception on reuse)
				view.Handler?.DisconnectHandler(); 
				handler.PlatformView.Content = view.ToPlatform(handler.MauiContext);
			}
				
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
