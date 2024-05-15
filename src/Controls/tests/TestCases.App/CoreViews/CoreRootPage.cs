using System;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	internal class CoreRootPage : Microsoft.Maui.Controls.ContentPage
	{
		public CoreRootPage(Microsoft.Maui.Controls.Page rootPage)
		{
			Title = "Core Gallery";

			var corePageView = new CorePageView(rootPage);

			var searchBar = new Microsoft.Maui.Controls.Entry()
			{
				AutomationId = "SearchBar"
			};

			searchBar.TextChanged += (sender, e) =>
			{
				corePageView.FilterPages(e.NewTextValue);
			};

			var testCasesButton = new Microsoft.Maui.Controls.Button
			{
				Text = "Go to Test Cases",
				AutomationId = "GoToTestButton",
				Command = new Microsoft.Maui.Controls.Command(async () =>
				{
					if (!string.IsNullOrEmpty(searchBar.Text))
					{
						await corePageView.NavigateToTest(searchBar.Text);
					}
					else
					{
						await Navigation.PushModalAsync(TestCases.GetTestCases());
					}
				})
			};

			var stackLayout = new Microsoft.Maui.Controls.StackLayout()
			{
				Children = {
					testCasesButton,
					searchBar,
					new Microsoft.Maui.Controls.Button {
						Text = "Click to Force GC",
						Command = new Microsoft.Maui.Controls.Command(() => {
							GC.Collect ();
							GC.WaitForPendingFinalizers ();
							GC.Collect ();
						})
					},
					corePageView
				}
			};

			AutomationId = "Gallery";

			Content = stackLayout;
		}
	}
}