using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
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

