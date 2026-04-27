namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 9095,
 "Shell toolbar back button doesn't fire Shell.OnBackButtonPressed on Android and iOS",
 PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue9095 : TestShell
{
	internal static bool BackButtonPressedCalledReturnFalse;
	internal static bool ContentPageBackButtonPressedCalledReturnFalse;

	protected override void Init()
	{
		Routing.RegisterRoute(nameof(Issue9095SecondPage), typeof(Issue9095SecondPage));
		Routing.RegisterRoute(nameof(Issue9095ReturnFalsePage), typeof(Issue9095ReturnFalsePage));
		AddContentPage(new Issue9095RootPage());
	}

	protected override bool OnBackButtonPressed()
	{
		// Set static flag to prove Shell.OnBackButtonPressed was called
		BackButtonPressedCalledReturnFalse = true;

		if (CurrentPage is Issue9095SecondPage secondPage)
		{
			secondPage.UpdateStatus("OnBackButtonPressed Called");
		}

		// Delegate to base which calls ContentPage.OnBackButtonPressed internally.
		// If ContentPage returns true → base returns true (navigation blocked).
		// If ContentPage returns false → base pops the stack (navigation proceeds).
		return base.OnBackButtonPressed();
	}


	public class Issue9095RootPage : ContentPage
	{
		readonly Label _returnFalseStatusLabel;
		readonly Label _contentPageReturnFalseStatusLabel;

		public Issue9095RootPage()
		{
			Title = "HomePage";
			BackButtonPressedCalledReturnFalse = false;
			ContentPageBackButtonPressedCalledReturnFalse = false;

			var navigateButton = new Button
			{
				Text = "Go to Second Page",
				AutomationId = "NavigateButton"
			};

			navigateButton.Clicked += async (s, e) =>
			 await Shell.Current.GoToAsync(nameof(Issue9095SecondPage));

			var navigateReturnFalseButton = new Button
			{
				Text = "Go to Return False Page",
				AutomationId = "NavigateReturnFalseButton"
			};

			navigateReturnFalseButton.Clicked += async (s, e) =>
			 await Shell.Current.GoToAsync(nameof(Issue9095ReturnFalsePage));

			_returnFalseStatusLabel = new Label
			{
				Text = "Waiting",
				AutomationId = "ReturnFalseStatusLabel"
			};

			_contentPageReturnFalseStatusLabel = new Label
			{
				Text = "Waiting",
				AutomationId = "ContentPageReturnFalseStatusLabel"
			};

			Content = new VerticalStackLayout
			{
				Children = { navigateButton, navigateReturnFalseButton, _returnFalseStatusLabel, _contentPageReturnFalseStatusLabel }
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (BackButtonPressedCalledReturnFalse)
			{
				_returnFalseStatusLabel.Text = "OnBackButtonPressed Called And Returned False";
				BackButtonPressedCalledReturnFalse = false;
			}
			if (ContentPageBackButtonPressedCalledReturnFalse)
			{
				_contentPageReturnFalseStatusLabel.Text = "ContentPage OnBackButtonPressed Called And Returned False";
				ContentPageBackButtonPressedCalledReturnFalse = false;
			}
		}
	}

	public class Issue9095SecondPage : ContentPage
	{
		readonly Label _statusLabel;
		readonly Label _contentPageStatusLabel;

		public Issue9095SecondPage()
		{
			Title = "Second Page";

			_statusLabel = new Label
			{
				Text = "OnBackButtonPressed Not Called",
				AutomationId = "BackButtonPressedLabel"
			};

			_contentPageStatusLabel = new Label
			{
				Text = "ContentPage OnBackButtonPressed Not Called",
				AutomationId = "ContentPageBackButtonLabel"
			};

			Content = new VerticalStackLayout
			{
				Children = { _statusLabel, _contentPageStatusLabel }
			};
		}

		public void UpdateStatus(string text)
		{
			_statusLabel.Text = text;
		}

		protected override bool OnBackButtonPressed()
		{
			_contentPageStatusLabel.Text = "ContentPage OnBackButtonPressed Called";
			return true;
		}
	}

	public class Issue9095ReturnFalsePage : ContentPage
	{
		public Issue9095ReturnFalsePage()
		{
			Title = "Return False Page";

			Content = new VerticalStackLayout
			{
				Children =
	{
	 new Label
	 {
	  Text = "Press back to test OnBackButtonPressed returning false",
	  AutomationId = "ReturnFalsePageLabel"
	 }
	}
			};
		}

		protected override bool OnBackButtonPressed()
		{
			ContentPageBackButtonPressedCalledReturnFalse = true;
			return false;
		}
	}
}
