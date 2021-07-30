using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ViewHandler<IView, PagePanel>
	{
		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			NativeView.CrossPlatformMeasure = VirtualView.Measure;
			NativeView.CrossPlatformArrange = VirtualView.Arrange;
		}

		void UpdateContent()
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.Children.Clear();

			if (VirtualView is IContentView cv && cv.Content is IView view)
				NativeView.Children.Add(view.ToNative(MauiContext));
		}

		protected override PagePanel CreateNativeView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
			}

			var view = new PagePanel
			{
				CrossPlatformMeasure = VirtualView.Measure,
				CrossPlatformArrange = VirtualView.Arrange
			};

			return view;
		}

		public static void MapTitle(PageHandler handler, IView page)
		{
		}

		public static void MapContent(PageHandler handler, IView page)
		{
			handler.UpdateContent();
		}
	}
}
