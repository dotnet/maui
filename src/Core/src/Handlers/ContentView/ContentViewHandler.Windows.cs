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

            _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
            _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

            PlatformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
            PlatformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
        }

        void UpdateContent()
        {
            _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
            _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
            _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            PlatformView.Children.Clear();

            if (VirtualView.PresentedContent is IView view)
                PlatformView.Children.Add(view.ToPlatform(MauiContext));
        }

        protected override ContentPanel CreatePlatformView()
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
