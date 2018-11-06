using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2628, "Unable to change page BackgroundImage from code", PlatformAffected.Android)]
	public class Issue2628 : ContentPage
	{
		public Issue2628 ()
		{
			var button1 = new Button { Text = "Click !!!!!!!!!!"};
			BackgroundImage="bank.png";
			button1.Clicked += ButtonAction;

			Content = new StackLayout {
				Spacing = 10,
				VerticalOptions = LayoutOptions.Center,
				Children = {
					button1
				}
			};
		}

		public  void ButtonAction(object sender, EventArgs args)
		{
			BackgroundImage="calculator.png";
		}
	}
}

