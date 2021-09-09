using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
    public partial class ContentViewHandler : ViewHandler<IContentView, ContentPanel>
    {
        public override void SetVirtualView(IView view)
        {
            base.SetVirtualView(view);

            _ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
            _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

            NativeView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
            NativeView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
        }

        void UpdateContent()
        {
            _ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
            _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
            _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            NativeView.Children.Clear();

            if (VirtualView.PresentedContent is IView view)
                NativeView.Children.Add(view.ToNative(MauiContext));
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

        public static void MapContent(ContentViewHandler handler, IContentView page)
        {
            handler.UpdateContent();
        }
    }
}
