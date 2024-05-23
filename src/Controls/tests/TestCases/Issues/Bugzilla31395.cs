using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 31395, "Crash when switching MainPage and using a Custom Render")]
	public class Bugzilla31395 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			Content = new CustomContentView
			{ // Replace with ContentView and everything works fine
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children = {
						new Button {
							Text = "Switch Main Page",
							Command = new Command (() => SwitchMainPage ())
						}
					}
				}
			};
		}

		void SwitchMainPage()
		{
			Application.Current.MainPage = new ContentPage { Content = new Label { Text = "Hello" } };
		}

		public class CustomContentView : ContentView
		{

		}
	}
}
