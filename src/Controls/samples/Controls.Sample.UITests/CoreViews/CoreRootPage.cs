using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	internal class CoreRootPage : ContentPage
	{
		public CoreRootPage(Page rootPage)
		{
			Title = "Core Gallery";

			var corePageView = new CorePageView(rootPage);

			var resetCheckBox = new CheckBox
			{
				AutomationId = "ResetMainPage",
				VerticalOptions = LayoutOptions.Center,
			};
			var resetLabel = new Label
			{
				Text = "Reset MainPage",
				VerticalOptions = LayoutOptions.Center,
			};
			var resetLayout = new HorizontalStackLayout
			{
				resetCheckBox,
				resetLabel
			};
			resetLayout.HorizontalOptions = LayoutOptions.End;
			resetLayout.Margin = new Microsoft.Maui.Thickness(12);

			var searchBar = new Entry()
			{
				AutomationId = "SearchBar"
			};

			searchBar.TextChanged += (sender, e) =>
			{
				corePageView.FilterPages(e.NewTextValue);
			};

			var testCasesButton = new Button
			{
				Text = "Go to Test Cases",
				AutomationId = "GoToTestButton",
				Command = new Command(async () =>
				{
					bool resetMainPage = resetCheckBox.IsChecked;

					if (!string.IsNullOrEmpty(searchBar.Text))
					{
						try
						{
							var testCaseScreen = new TestCases.TestCaseScreen(resetMainPage);
							await Task.Delay(100); // Load all the issues before try to navigate.

							if (TestCases.TestCaseScreen.PageToAction.ContainsKey(searchBar.Text?.Trim()))
							{
								TestCases.TestCaseScreen.PageToAction[searchBar.Text?.Trim()]();
							}
							else if (!testCaseScreen.TryToNavigateTo(searchBar.Text?.Trim()))
							{
								throw new Exception($"Unable to Navigate to {searchBar.Text}");
							}
						}
						catch (Exception e)
						{
							System.Diagnostics.Debug.WriteLine(e.Message);
							Console.WriteLine(e.Message);
						}
					}
					else
					{
						await Navigation.PushModalAsync(TestCases.GetTestCases(resetMainPage));
					}
				})
			};

			var stackLayout = new StackLayout()
			{
				Children = {
					resetLayout,
					testCasesButton,
					searchBar,
					new Button {
						Text = "Click to Force GC",
						Command = new Command(() => {
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