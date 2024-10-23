namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Bugzilla, 25979, "https://bugzilla.xamarin.com/show_bug.cgi?id=25979")]
public class Bugzilla25979 : TestNavigationPage
{
	internal sealed class MyPage : ContentPage
	{
		public MyPage()
		{
			Title = "Page 1";
			AutomationId = "PageOneId";

			var b = new Button
			{
				AutomationId = "PageOneButtonId",
				Text = "Next"
			};
			b.Clicked += async (sender, e) =>
			{
				await Navigation.PushAsync(new MyPage2());
			};

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
			Content = new StackLayout
			{
				BackgroundColor = Colors.Red,
				Children = {
					new Label { Text = "Page 1", FontSize=Device.GetNamedSize(NamedSize.Large, typeof(Label)) },
					b
				}
			};
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
		}
	}


	internal sealed class MyPage2 : ContentPage
	{
		public MyPage2()
		{
			Title = "Page 2";
			AutomationId = "PageTwoId";

			var b = new Button
			{
				AutomationId = "PageTwoButtonId",
				Text = "Next"
			};
			b.Clicked += async (sender, e) =>
			{
				await Navigation.PushAsync(new MyPage3());
				Navigation.NavigationStack[0].BindingContext = null;
				Navigation.RemovePage(Navigation.NavigationStack[0]);
			};

#pragma warning disable CS0612 // Type or member is obsolete
			Content = new StackLayout
			{
				BackgroundColor = Colors.Red,
				Children = {
					new Label { Text = "Page 2", FontSize=Device.GetNamedSize(NamedSize.Large, typeof(Label)) },
					b
				}
			};
#pragma warning restore CS0612 // Type or member is obsolete
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Navigation.NavigationStack[0].BindingContext = null;
			Navigation.RemovePage(Navigation.NavigationStack[0]);
		}
	}


	internal sealed class MyPage3 : ContentPage
	{
		public MyPage3()
		{
			AutomationId = "PageThreeId";
			Title = "PageThreeId";

			var label = new Label { Text = "Page 3" };

			Content = new StackLayout
			{
				Children = {
					label,
					new Button {
						AutomationId = "PopButton",
						Text = "Try to Pop",
						Command = new Command(
							async() => {
								await Navigation.PopAsync();
								label.Text = "PopAttempted";
							}
						)}
					}
			};
		}
	}

	protected override void Init()
	{
		// Initialize ui here instead of ctor
		Navigation.PushAsync(new MyPage() { Title = "Navigation Stack" });
	}
}
