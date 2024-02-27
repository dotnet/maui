using System;
using System.Diagnostics;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Handlers
{

	public partial class ViewHandler<TVirtualView, TPlatformView> : IPlatformViewHandler
	{

		Gtk.Widget? IPlatformViewHandler.PlatformView => (Gtk.Widget?)base.PlatformView;

		public override void PlatformArrange(Rect rect)
		{
			this.PlatformArrangeHandler(rect);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
			=> this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

		protected override void SetupContainer()
		{ }

		protected override void RemoveContainer()
		{ }

		protected void InvokeEvent(Action action)
		{
			Dispatcher.Invoke(action);
		}

		public void MapFont(ITextStyle textStyle)
		{
			MapFont(PlatformView, textStyle);

		}

		public void MapFont(Gtk.Widget? platformView, ITextStyle textStyle)
		{
			if (platformView == null)
				return;

			var fontManager = this.GetRequiredService<IFontManager>();
			platformView.UpdateFont(textStyle, fontManager);

		}

	}

}