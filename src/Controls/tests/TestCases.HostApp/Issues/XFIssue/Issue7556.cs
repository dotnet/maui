namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7556, "[iOS] Masterbehavior.popover not being observed on iOS 13", PlatformAffected.iOS)]

public class Issue7556 : TestFlyoutPage
{
	protected override void Init()
	{
		var closeFlyout = new Command(() => IsPresented = false,
			() => Window is not null && !((IFlyoutPageController)this).ShouldShowSplitMode);

		SizeChanged += (_, _) => closeFlyout.ChangeCanExecute();
		PropertyChanged += (_, args) =>
		{
			if (args.PropertyName == nameof(FlyoutLayoutBehavior))
				closeFlyout.ChangeCanExecute();
		};

		Flyout = new ContentPage()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label() { Margin = 20, Text = "Flyout Visible", AutomationId = "FlyoutVisible", TextColor = Colors.White },
					// The phone flyout covers the detail page, so these actions must be reachable here.
					new Button()
					{
						Text = "Push Modal Page When on Split FlyoutLayoutBehavior",
						AutomationId = "PushModalPage",
						Command = new Command(() =>
						{
							Detail.Navigation.PushModalAsync(new ContentPage()
							{
								Content = new Button()
								{
									Text = "After popping this Page FlyoutLayoutBehavior should still be split",
									AutomationId = "PopModalPage",
									Command = new Command(() => Detail.Navigation.PopModalAsync())
								}
							});
						})
					},
					new Button()
					{
						Text = "Close Flyout",
						AutomationId = "CloseFlyout",
						Command = closeFlyout
					}
				}
			},
			Title = "Flyout",
			BackgroundColor = Colors.Blue
		};

		Detail = new NavigationPage(new DetailsPage(this) { Title = "Details" });
	}

	public class DetailsPage : ContentPage
	{
		FlyoutPage MDP { get; }
		Label lblThings;

		public DetailsPage(FlyoutPage FlyoutPage)
		{
			MDP = FlyoutPage;
			lblThings = new Label() { HorizontalTextAlignment = TextAlignment.Center, AutomationId = "CurrentMasterBehavior" };

			Content = new StackLayout()
			{
				lblThings,
				new Button()
				{
					Text = "Click to rotate through FlyoutLayoutBehavior settings and test each one",
					Command = new Command(OnChangeMasterBehavior),
					AutomationId = "ChangeMasterBehavior"
				},
				new Label(){ HorizontalTextAlignment = TextAlignment.Center, Text = "Close Flyout", AutomationId = "DetailSentinel" },
				new Label()
				{
					Text = DeviceInfo.Idiom.ToString(),
					AutomationId = "DeviceIdiom"
				}
			};


			MDP.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;
			lblThings.Text = MDP.FlyoutLayoutBehavior.ToString();
		}

		void OnChangeMasterBehavior()
		{
			var behavior = MDP.FlyoutLayoutBehavior;
			var results = Enum.GetValues(typeof(FlyoutLayoutBehavior)).Cast<FlyoutLayoutBehavior>().ToList();

			int nextIndex = results.IndexOf(behavior) + 1;
			if (nextIndex >= results.Count)
				nextIndex = 0;

			MDP.FlyoutLayoutBehavior = results[nextIndex];
			lblThings.Text = MDP.FlyoutLayoutBehavior.ToString();
		}
	}
}
