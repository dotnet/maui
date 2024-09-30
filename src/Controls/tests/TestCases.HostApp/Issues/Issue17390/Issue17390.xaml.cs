using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 17390, "Shell where the bottom padding is not calculated properly when navigating from a tabbed page to a non-tabbed page and returning back to the tabbed page.",
		PlatformAffected.Android)]
	public partial class Issue17390 : Shell
	{
		public Issue17390()
		{
			InitializeComponent();
			Routing.RegisterRoute("nontabbedpage", typeof(NonTabbedPage));
			Routing.RegisterRoute("innertabbedpage", typeof(InnerTabbedPage));
		}
		async void OpenNonTabbedPage(object sender, EventArgs args)
		{
			await Shell.Current.GoToAsync("nontabbedpage");
		}

		async void OpenInnerTabbedPage(object sender, EventArgs args)
		{
			await Shell.Current.GoToAsync("innertabbedpage");
		}
	}
}