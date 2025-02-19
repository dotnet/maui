using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 13916, "[iOS] iOS Application crashes on Back press when navigated to using GoToAsync with \"//\" or \"///\" route if 2 or more things are removed from the navigation stack",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Github5000)]
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue13916 : TestShell
	{
		static int pageCount = 1;
		protected override void Init()
		{
			Routing.RegisterRoute(nameof(Issue13916SuccessPage), typeof(Issue13916SuccessPage));

			AddFlyoutItem(CreateContentPage(), "Push Me");
		}


		public class Issue13916SuccessPage : ContentPage
		{
			public Issue13916SuccessPage()
			{
				StackLayout layout = new StackLayout();
				Label label = new Label()
				{
					Text = "Success",
					AutomationId = "Success"
				};
				layout.Children.Add(label);
				Content = layout;
			}
		}

		ContentPage CreateContentPage()
		{
			StackLayout layout = new StackLayout();
			Button button = new Button()
			{
				Text = "Click Me",
				AutomationId = $"ClickMe{pageCount}",
				Command = new Command(async () =>
				{
					if (Navigation.NavigationStack.Count >= 3)
					{
						await GoToAsync($"../../{nameof(Issue13916SuccessPage)}");
					}
					else
					{
						await Navigation.PushAsync(CreateContentPage());
					}
				})
			};
			pageCount++;

			layout.Children.Add(button);

			return new ContentPage()
			{
				Content = layout
			};
		}

#if UITEST
		[Test]
		public void RemovingMoreThanOneInnerPageAndThenPushingAPageCrashes()
		{
			RunningApp.Tap("ClickMe1");
			RunningApp.Tap("ClickMe2");
			RunningApp.Tap("ClickMe3");
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}
