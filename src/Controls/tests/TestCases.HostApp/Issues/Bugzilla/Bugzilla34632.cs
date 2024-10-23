namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 34632, "Can't change IsPresented when setting SplitOnLandscape ")]
public class Bugzilla34632 : TestFlyoutPage
{
	protected override void Init()
	{
#pragma warning disable CS0618 // Type or member is obsolete
		if (DeviceInfo.Platform == DevicePlatform.UWP)
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;
		else
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.SplitOnLandscape;
#pragma warning restore CS0618 // Type or member is obsolete

		Flyout = new ContentPage
		{
			Title = "Main Page",
			Content = new Button
			{
				Text = "Flyout",
				AutomationId = "btnFlyout",
				Command = new Command(() =>
				{
					//If we're in potrait toggle hide the menu on click
					if (Width < Height || DeviceInfo.Idiom == DeviceIdiom.Phone)
					{
						IsPresented = false;
					}
				})
			}
		};

		Detail = new NavigationPage(new ModalRotationIssue());
		NavigationPage.SetHasBackButton(Detail, false);
	}


	public class ModalRotationIssue : ContentPage
	{
		public ModalRotationIssue()
		{
			var btn = new Button { Text = "Open Modal", AutomationId = "btnModal" };
			btn.Clicked += OnButtonClicked;
			Content = btn;
		}

		async void OnButtonClicked(object sender, EventArgs e)
		{
			var testButton = new Button { Text = "Rotate Before Clicking", AutomationId = "btnDismissModal" };
			testButton.Clicked += (async (snd, args) => await Navigation.PopModalAsync());

			var testModal = new ContentPage()
			{
				Content = testButton
			};

			await Navigation.PushModalAsync(testModal);
		}
	}
}
