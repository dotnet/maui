using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{	
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1653, "ScrollView exceeding bounds - v2", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public partial class Issue1653v2 : ContentPage
	{	
		public Issue1653v2 ()
		{
			InitializeComponent ();

			for (int i = 0; i < 40; i++)
				addonGroupStack.Children.Add (new Label {Text = "Testing 123"});
		}
	}
#endif
}

