using System;
using System.Collections.Generic;
using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{	
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1497, "Grid sizing issue", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public partial class Issue1497 : ContentPage
	{	
		public Issue1497 ()
		{
			InitializeComponent ();
		}
	}
#endif
}

