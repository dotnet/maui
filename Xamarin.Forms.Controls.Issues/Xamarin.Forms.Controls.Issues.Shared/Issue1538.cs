using System;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1538, "Crash measuring empty ScrollView", PlatformAffected.Android | PlatformAffected.WinPhone)]
	public class Issue1538 : ContentPage
	{
		ScrollView _sv;
		public Issue1538 ()
		{
			StackLayout sl = new StackLayout(){VerticalOptions = LayoutOptions.FillAndExpand};
			sl.Children.Add( _sv = new ScrollView(){HeightRequest=100} );
			Content = sl;

			AddContentDelayed ();
		}

		async void AddContentDelayed ()
		{
			await Task.Delay (1000);
			_sv.Content = new Label { Text = "Foo" };
		}
	}
}
