using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1722, "MasterDetailPage crashes when assigning a NavigationPage to Detail with no children pushed", PlatformAffected.iOS)]
	public class Issue1722 : MasterDetailPage
	{
		public Issue1722 ()
		{
			Master = new ContentPage {
				Title = "Master",
				Content = new Label { Text = "Master" }
			};

			Detail = new NavigationPage ();
		}
	}
}
