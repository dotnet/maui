using System;
using System.Collections.Generic;
using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{	
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1712, "Wrong error thrown when setting LayoutOptions property to string", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public partial class Issue1712 : ContentPage
	{	
		public Issue1712 ()
		{
			InitializeComponent ();
		}
	}
#endif
}

