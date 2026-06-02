namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 27799, "[iOS] OnAppearing and OnNavigatedTo does not work when using extended Tabbar", PlatformAffected.iOS)]

	public class Issue27799 : TestShell
	{
		private static int _onNavigatedToCount = 0;
		private static int _onAppearingCount = 0;

		protected override void Init()
		{
			Routing.RegisterRoute(nameof(Tab6Subpage), typeof(Tab6Subpage));
			AddBottomTab("tab1");
			AddBottomTab(new Tab2(), "tab2");
			AddBottomTab("tab3");
			AddBottomTab("tab4");
			AddBottomTab("tab5");
			AddBottomTab(new Tab6(), "Tab6");
		}

		class Tab2 : ContentPage
		{
			Label _onNavigatedToCountLabel;
			Label _onAppearingCountLabel;
			public Tab2()
			{
				_onNavigatedToCountLabel = new Label { AutomationId = "OnNavigatedToCountLabel", Text = $"OnNavigatedTo: {_onNavigatedToCount}" };
				_onAppearingCountLabel = new Label { AutomationId = "OnAppearingCountLabel", Text = $"OnAppearing: {_onAppearingCount}" };
				Content = new StackLayout
				{
					Children =
					{
						_onNavigatedToCountLabel,
						_onAppearingCountLabel
					}
				};
			}

			protected override void OnNavigatedTo(NavigatedToEventArgs e)
			{
				_onNavigatedToCount++;
				_onNavigatedToCountLabel.Text = $"OnNavigatedTo: {_onNavigatedToCount}";
			}

			protected override void OnAppearing()
			{
				_onAppearingCount++;
				_onAppearingCountLabel.Text = $"OnAppearing: {_onAppearingCount}";
			}
		}

		class Tab6 : ContentPage
		{
			public Tab6()
			{
				Content = new Button
				{
					Text = "Go to subpage6",
					AutomationId = "GoToSubpage6Button",
					Command = new Command(async () => await Current.GoToAsync(nameof(Tab6Subpage)))
				};
			}
			protected override void OnNavigatedTo(NavigatedToEventArgs e) => _onNavigatedToCount++;
			protected override void OnAppearing() => _onAppearingCount++;
		}

		class Tab6Subpage : ContentPage
		{
			protected override void OnNavigatedTo(NavigatedToEventArgs e) => _onNavigatedToCount++;
			protected override void OnAppearing() => _onAppearingCount++;
		}
	}
}