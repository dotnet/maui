using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1741, "Switch.IsEnabled = false does not disable a switch when switch is defined in XAML", PlatformAffected.WinPhone)]
	public partial class Issue1741 : ContentPage
	{
		public Issue1741 ()
		{
			InitializeComponent();
		}

		public void Anonymous_Toggled (object sender, EventArgs e)
		{
			chkAnon.IsEnabled = !chkAnon.IsEnabled;
		}
	}
#endif
}
