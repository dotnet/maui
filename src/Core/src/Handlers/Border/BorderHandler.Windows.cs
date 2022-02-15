using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
    public partial class BorderHandler : ViewHandler<IBorderView, ContentPanel>
    {
        public override void SetVirtualView(IView view)
        {
            base.SetVirtualView(view);

            _ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
            _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

            NativeView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
            NativeView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
        }

        static void UpdateContent(IBorderHandler handler)
        {
            _ = handler.NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
            _ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
            _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			handler.NativeView.Children.Clear();
			handler.NativeView.EnsureBorderPath();

            if (handler.VirtualView.PresentedContent is IView view)
				handler.NativeView.Children.Add(view.ToPlatform(handler.MauiContext));
        }

        protected override ContentPanel CreateNativeView()
        {
            if (VirtualView == null)
            {
                throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
            }

            var view = new ContentPanel
			{
                CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
                CrossPlatformArrange = VirtualView.CrossPlatformArrange
            };

            return view;
        }

		public static void MapContent(IBorderHandler handler, IBorderView border)
		{
			UpdateContent(handler);
		}
	}
}
