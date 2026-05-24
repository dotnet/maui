namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 53179,
		"PopAsync crashing after RemovePage when support packages are updated to 25.1.1", PlatformAffected.Android)]
	public class Bugzilla53179 : NavigationPage
	{
		public Bugzilla53179() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(new TestPage(1));
			}

			class TestPage : ContentPage
			{
				Button nextBtn, rmBtn, popBtn;

				public TestPage(int index)
				{
					nextBtn = new Button { AutomationId = $"Next Page {index}", Text = $"Next Page {index}" };
					rmBtn = new Button { AutomationId = "Remove previous pages", Text = "Remove previous pages" };
					popBtn = new Button { AutomationId = "Back", Text = "Back" };

					nextBtn.Clicked += async (sender, e) => await Navigation.PushAsync(new TestPage(index + 1));
					rmBtn.Clicked += (sender, e) =>
					{
						var stackSize = Navigation.NavigationStack.Count;
						Navigation.RemovePage(Navigation.NavigationStack[stackSize - 2]);

						stackSize = Navigation.NavigationStack.Count;
						Navigation.RemovePage(Navigation.NavigationStack[stackSize - 2]);

						popBtn.IsVisible = true;
						rmBtn.IsVisible = false;
					};
					popBtn.Clicked += async (sender, e) => await Navigation.PopAsync();

					switch (index)
					{
						case 4:
							nextBtn.IsVisible = false;
							popBtn.IsVisible = false;
							break;
						default:
							rmBtn.IsVisible = false;
							popBtn.IsVisible = false;
							break;
					}

					Content = new StackLayout
					{
						Children = {
							new Label { Text = $"This is page {index}"},
							nextBtn,
							rmBtn,
							popBtn
						}
					};
				}
			}
		}
	}
}