using System;
using Gtk;
using Microsoft.Maui.Native;

namespace Microsoft.Maui.Handlers
{

	public partial class PageHandler : ViewHandler<IView, PageView>
	{

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.CrossPlatformMeasure = VirtualView.Measure;
			NativeView.CrossPlatformArrange = VirtualView.Arrange;

		}

		void UpdateContent()
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (VirtualView is IContentView { Content: { } view })
				NativeView.Content = view.ToNative(MauiContext);
		}

		protected override PageView CreateNativeView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
			}

			var pw = new PageView
			{
				CrossPlatformMeasure = VirtualView.Measure,
				CrossPlatformArrange = VirtualView.Arrange
			};

			return pw;
		}

		public static void MapContent(PageHandler handler, IView page)
		{
			handler.UpdateContent();
		}

		[MissingMapper]
		public static void MapTitle(PageHandler handler, IView page)
		{ }

	}

}