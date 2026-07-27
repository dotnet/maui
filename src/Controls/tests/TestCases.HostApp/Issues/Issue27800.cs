#if ANDROID || IOS
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27800, "Shell.BackButtonBehavior does not work when using extended Tabbar", PlatformAffected.iOS)]
public class Issue27800 : TestShell
{

	protected override void Init()
	{
		Routing.RegisterRoute(nameof(Tab6DetailPage), typeof(Tab6DetailPage));
		AddBottomTab("tab1");
		AddBottomTab("tab2");
		AddBottomTab("tab3");
		AddBottomTab("tab4");
		AddBottomTab("tab5");
		AddBottomTab(new Tab6(), "tab6");
	}

	class Tab6 : ContentPage
	{
		Label _onNavigatedToCountLabel;
		Label _onAppearingCountLabel;
		int _onNavigatedToCount;
		int _onAppearingCount;

		public Tab6()
		{
			_onNavigatedToCountLabel = new Label { AutomationId = "OnNavigatedToCountLabel", Text = $"OnNavigatedTo: {_onNavigatedToCount}" };
			_onAppearingCountLabel = new Label { AutomationId = "OnAppearingCountLabel", Text = $"OnAppearing: {_onAppearingCount}" };
			Content = new StackLayout
			{
				Children =
					{
						_onNavigatedToCountLabel,
						_onAppearingCountLabel,
						new Button
						{
							AutomationId = "button",
							Text = "Tap to navigate to a detail page",
							Command = new Command(() => Shell.Current.GoToAsync(nameof(Tab6DetailPage)))
						}
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


	class Tab6DetailPage : ContentPage
	{
		public Tab6DetailPage()
		{
			Shell.SetBackButtonBehavior(this, new BackButtonBehavior
			{
				TextOverride = "Go Back",
				Command = new Command(() => Shell.Current.GoToAsync(".."))
			});
		}
	}
}
#endif