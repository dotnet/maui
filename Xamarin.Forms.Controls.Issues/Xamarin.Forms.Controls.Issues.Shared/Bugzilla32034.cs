using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Bugzilla, 32034, "MissingMethodException while pushing and popping pages",PlatformAffected.Android, NavigationBehavior.PushModalAsync)]
	public class Bugzilla32034 : NavigationPage
	{
		public class ButtonPage : ContentPage
		{
			public ButtonPage ()
			{
				Content = new StackLayout {
					Children = {
						new Button {
							Text = "Push", Command = new Command (o => ((NavigationPage) Parent).PushAsync (new ButtonPage ()))
						},
						new Button {
							Text = "Pop", Command = new Command (o => ((NavigationPage) Parent).Navigation.PopAsync ())
						},
					},
				};
			}
		}

		public Bugzilla32034 () : base (new ButtonPage ())
		{
		}
	}
}
