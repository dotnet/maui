using System;
using System.Diagnostics;
using Gdk;
using Microsoft.Maui.Graphics.Native.Gtk;
using Rectangle = Microsoft.Maui.Graphics.Rectangle;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Handlers
{

	public partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{

		Gtk.Widget? INativeViewHandler.NativeView => (Gtk.Widget?)base.NativeView;

		public override void NativeArrange(Rectangle rect)
		{
			NativeView?.Arrange(rect);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
			=> NativeView.GetDesiredSize(widthConstraint, heightConstraint);

		protected override void SetupContainer()
		{ }

		protected override void RemoveContainer()
		{ }

		protected void InvokeEvent(Action action)
		{
			MauiGtkApplication.Invoke(action);
		}

		public void MapFont(ITextStyle textStyle)
		{
			MapFont(NativeView, textStyle);

		}

		public void MapFont(Gtk.Widget? nativeView, ITextStyle textStyle)
		{
			if (nativeView == null)
				return;

			var fontManager = this.GetRequiredService<IFontManager>();
			nativeView.UpdateFont(textStyle, fontManager);

		}

	}

}