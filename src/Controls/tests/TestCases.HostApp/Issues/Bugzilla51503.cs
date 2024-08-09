using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 51503, "NullReferenceException on VisualElement Finalize", PlatformAffected.All)]
	public class Bugzilla51503 : NavigationPage
	{
		public Bugzilla51503() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(new _51503RootPage());
			}

			[Preserve(AllMembers = true)]
			class _51503RootPage : ContentPage
			{
				public _51503RootPage()
				{
					Button button = new Button
					{
						AutomationId = "Button",
						Text = "Open"
					};

					button.Clicked += Button_Clicked;

					Content = button;
				}

				async void Button_Clicked(object sender, EventArgs e)
				{
					GarbageCollectionHelper.Collect();

					await Navigation.PushAsync(new ChildPage());
				}
			}

			[Preserve(AllMembers = true)]
			class ChildPage : ContentPage
			{
				public ChildPage()
				{
					Content = new Label
					{
						AutomationId = "VisualElement",
						Text = "Navigate 3 times to this page",
						Triggers =
					{
						new EventTrigger()
					}
					};
				}
			}
		}
	}
}