using System;
using Android.App;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>
	{
		public static void MapTitle(WindowHandler handler, IWindow window) { }

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var nativeContent = VirtualView.Content.ToContainerView(MauiContext);

			NativeView.SetContentView(nativeContent);
		}
	}
}