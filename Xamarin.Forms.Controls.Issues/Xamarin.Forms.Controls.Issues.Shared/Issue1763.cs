using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1763, "First item of grouped ListView not firing .ItemTapped", PlatformAffected.WinPhone, NavigationBehavior.PushAsync)]
	public class Issue1763 : TabbedPage
	{
		public Issue1763 ()
		{
			Title = "Contacts";

			Children.Add (new ContactsPage ());
		}
	}
}
