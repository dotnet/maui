using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentViewHandler : ViewHandler<IContentView, ContentViewGroup>
	{
		protected override ContentViewGroup CreatePlatformView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a ContentViewGroup");
			}

			var viewGroup = new ContentViewGroup(Context)
			{
				CrossPlatformLayout = VirtualView
			};

			viewGroup.SetClipChildren(false);

			return viewGroup;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.CrossPlatformLayout = VirtualView;
		}

		static void UpdateContent(IContentViewHandler handler)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			// If the outgoing content (or one of its descendants, e.g. an Entry's EditText) still
			// holds native Android focus, clear it before ripping the view out of the hierarchy.
			// RemoveAllViews() does not go through DisconnectHandler/ClearFocus, so without this,
			// a focused EditText gets orphaned mid-focus: the InputMethodManager is left holding a
			// stale reference to a detached view instead of being told focus was relinquished. That
			// stale IME state can surface the next time this same Entry (or Content slot) regains
			// focus, briefly showing the previous keyboard layout before the correct one appears.
			handler.PlatformView.FindFocus()?.ClearFocus();

			handler.PlatformView.RemoveAllViews();

			if (handler.VirtualView.PresentedContent is IView view)
			{
				var platformView = view.ToPlatform(handler.MauiContext);
				// Ensure the view is detached from any existing parent before adding it
				platformView.RemoveFromParent();
				handler.PlatformView.AddView(platformView);
			}
		}

		public static partial void MapContent(IContentViewHandler handler, IContentView page)
		{
			UpdateContent(handler);
		}

		protected override void DisconnectHandler(ContentViewGroup platformView)
		{
			// If we're being disconnected from the xplat element, then we should no longer be managing its children
			platformView.CrossPlatformLayout = null;
			platformView.RemoveAllViews();
			base.DisconnectHandler(platformView);
		}
	}
}
