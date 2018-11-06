using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
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

