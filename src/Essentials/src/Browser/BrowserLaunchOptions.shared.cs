using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/BrowserLaunchOptions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.BrowserLaunchOptions']/Docs" />
	public class BrowserLaunchOptions
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/BrowserLaunchOptions.xml" path="//Member[@MemberName='PreferredToolbarColor']/Docs" />
		public Color PreferredToolbarColor { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/BrowserLaunchOptions.xml" path="//Member[@MemberName='PreferredControlColor']/Docs" />
		public Color PreferredControlColor { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/BrowserLaunchOptions.xml" path="//Member[@MemberName='LaunchMode']/Docs" />
		public BrowserLaunchMode LaunchMode { get; set; } = BrowserLaunchMode.SystemPreferred;

		/// <include file="../../docs/Microsoft.Maui.Essentials/BrowserLaunchOptions.xml" path="//Member[@MemberName='TitleMode']/Docs" />
		public BrowserTitleMode TitleMode { get; set; } = BrowserTitleMode.Default;

		/// <include file="../../docs/Microsoft.Maui.Essentials/BrowserLaunchOptions.xml" path="//Member[@MemberName='Flags']/Docs" />
		public BrowserLaunchFlags Flags { get; set; } = BrowserLaunchFlags.None;

		internal bool HasFlag(BrowserLaunchFlags flag)
			=> Flags.HasFlag(flag);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/BrowserLaunchFlags.xml" path="Type[@FullName='Microsoft.Maui.Essentials.BrowserLaunchFlags']/Docs" />
	[Flags]
	public enum BrowserLaunchFlags
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/BrowserLaunchFlags.xml" path="//Member[@MemberName='None']/Docs" />
		None = 0,
		/// <include file="../../docs/Microsoft.Maui.Essentials/BrowserLaunchFlags.xml" path="//Member[@MemberName='LaunchAdjacent']/Docs" />
		LaunchAdjacent = 1,
		/// <include file="../../docs/Microsoft.Maui.Essentials/BrowserLaunchFlags.xml" path="//Member[@MemberName='PresentAsPageSheet']/Docs" />
		PresentAsPageSheet = 2,
		/// <include file="../../docs/Microsoft.Maui.Essentials/BrowserLaunchFlags.xml" path="//Member[@MemberName='PresentAsFormSheet']/Docs" />
		PresentAsFormSheet = 4
	}
}
