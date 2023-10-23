using System;
using Gtk;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{

	public partial class PageHandler : ContentViewHandler
	{

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			PlatformView.CrossPlatformMeasure = VirtualView.Measure;
			PlatformView.CrossPlatformArrange = VirtualView.Arrange;

		}

		protected override ContentView CreatePlatformView()
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
		public static void MapTitle(IPageHandler handler, IContentView page) { }

	}

}