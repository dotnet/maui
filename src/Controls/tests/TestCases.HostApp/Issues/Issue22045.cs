namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22045, "[Android] OnSizeAllocated not reported for Android AppShell Flyout content", PlatformAffected.Android)]
public class Issue22045 : Shell
{
	public Issue22045()
	{
		this.FlyoutBehavior = FlyoutBehavior.Flyout;

		var shellContent = new ShellContent
		{
			Title = "Home",
			Route = "MainPage",
			ContentTemplate = new DataTemplate(typeof(_22045MainPage))
		};

		Items.Add(shellContent);

		this.FlyoutContent = new _22045NewContent();
	}

	public class _22045NewContent : ContentView
	{
		Label widthLabel;
		Label heightLabel;

		public _22045NewContent()
		{
			widthLabel = new Label
			{
				AutomationId = "WidthLabel",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			heightLabel = new Label
			{
				AutomationId = "HeightLabel",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			Content = new VerticalStackLayout
			{
				Children =
				{
					widthLabel,
					heightLabel
				}
			};
		}
		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			widthLabel.Text = width.ToString();
			heightLabel.Text = height.ToString();
		}
	}

	public class _22045MainPage : ContentPage
	{
		public _22045MainPage()
		{
			var button = new Button
			{
				Text = "Open Flyout",
				AutomationId = "OpenFlyoutButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			button.Clicked += (s, e) =>
			{
				Shell.Current.FlyoutIsPresented = true;
			};

			Content = new VerticalStackLayout
			{
				Children =
				{
					button
				}
			};
		}
	}
}
