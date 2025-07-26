using System;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25920, ".NET MAUI set AppShell custom FlyoutIcon display problem", PlatformAffected.iOS | PlatformAffected.Android)]
	public partial class Issue25920 : Shell
	{
		public Issue25920()
		{
			InitializeComponent();
		}
	}
}