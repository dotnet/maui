using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Essentials
{
	public class BrowserLaunchOptions
	{
		public Color PreferredToolbarColor { get; set; }

		public Color PreferredControlColor { get; set; }

		public BrowserLaunchMode LaunchMode { get; set; } = BrowserLaunchMode.SystemPreferred;

		public BrowserTitleMode TitleMode { get; set; } = BrowserTitleMode.Default;

		public BrowserLaunchFlags Flags { get; set; } = BrowserLaunchFlags.None;

		internal bool HasFlag(BrowserLaunchFlags flag)
			=> Flags.HasFlag(flag);
	}

	[Flags]
	public enum BrowserLaunchFlags
	{
		None = 0,
		LaunchAdjacent = 1,
		PresentAsPageSheet = 2,
		PresentAsFormSheet = 4
	}
}
