using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public static class NSButtonExtensions
	{
		public static NSButton CreateButton(string text, Action activate = null)
		{
			return CreateButton(text, null, activate);
		}

		public static NSButton CreateButton(string text, NSImage image = null, Action activate = null)
		{
			var btn = new NSButton { Title = text };
			btn.BezelStyle = NSBezelStyle.TexturedRounded;

			if (image != null)
				btn.Image = image;
			if (activate != null)
				btn.Activated += (sender, e) => activate();
			return btn;
		}
	}
}