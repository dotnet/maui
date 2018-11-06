using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1653, "ScrollView exceeding bounds", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public partial class Issue1653 : ContentPage
	{
		public Issue1653 ()
		{
			InitializeComponent ();

			for (int i = 0; i < 40; i++)
				addonGroupStack.Children.Add (new Label {Text = "Testing 123"});
		}
	}
#endif
}
