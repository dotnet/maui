using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Application = Microsoft.Maui.Controls.Application;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23411, "[iOS]Use non-overridden traits in AppInfoImplementation.RequestTheme",
		PlatformAffected.iOS)]
	public partial class Issue23411 : ContentPage
	{
		public Issue23411()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			Application.Current.UserAppTheme = AppTheme.Dark;
			base.OnAppearing();
		}
		private void OnModalPage(object sender, EventArgs e)
		{
#if IOS
			var modalPage = new Microsoft.Maui.Controls.NavigationPage(new Issue23411Pagesheet() { Title = "Modal Page", AutomationId = "PageSheetModalPage" });
			modalPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
			Navigation.PushModalAsync(modalPage);
#endif
		}

		private void OnResetTheme(object sender, EventArgs e)
		{
			Application.Current.UserAppTheme = AppTheme.Unspecified;
		}
	}

	public partial class Issue23411Pagesheet : ContentPage
	{
		private Label ThemeLabelText;
		private Button ResetThemeButton;

		public Issue23411Pagesheet()
		{
			InitializeComponents();
		}

		private void InitializeComponents()
		{
			// Create Label
			ThemeLabelText = new Label
			{
				Text = "Current Theme",
				AutomationId = "ThemePageLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};

			// Create Button
			ResetThemeButton = new Button
			{
				Text = "Reset Theme",
				AutomationId = "ResetThemePageButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
			ResetThemeButton.Clicked += OnResetTheme;

			// Create Layout
			var stackLayout = new StackLayout
			{
				Children = { ThemeLabelText, ResetThemeButton },
			};

			// Set the page content to the layout
			Content = stackLayout;
		}

		private void OnResetTheme(object sender, EventArgs e)
		{
			Application.Current.UserAppTheme = AppTheme.Unspecified;
			ThemeLabelText.Text = "Theme reset to Unspecified";
		}
	}
}
