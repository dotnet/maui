namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 362691, "ToolbarItem custom TintColor is lost when IsEnabled changes on iOS", PlatformAffected.iOS)]
	public class Issue362691 : NavigationPage
	{
		public Issue362691() : base(new MainPage())
		{
		}

		class MainPage : ContentPage
		{
			readonly ToolbarItem _saveItem;
			readonly ToolbarItem _refreshItem;

			public MainPage()
			{
				Title = "Issue 362691";

				_saveItem = new ToolbarItem
				{
					Text = "Save",
					Order = ToolbarItemOrder.Primary,
					Priority = 0,
					AutomationId = "SaveItem"
				};

				_refreshItem = new ToolbarItem
				{
					Text = "Refresh",
					Order = ToolbarItemOrder.Primary,
					Priority = 1,
					AutomationId = "RefreshItem"
				};

				ToolbarItems.Add(_saveItem);
				ToolbarItems.Add(_refreshItem);

				var toggleButton = new Button
				{
					Text = "Simulate Dialog (Disable -> Enable)",
					AutomationId = "ToggleIsEnabledButton"
				};

				toggleButton.Clicked += async (s, e) =>
				{
					_saveItem.IsEnabled = false;
					_refreshItem.IsEnabled = false;

					await DisplayAlertAsync("Popup", "Toolbar is disabled", "Dismiss");

					_saveItem.IsEnabled = true;
					_refreshItem.IsEnabled = true;
				};

				Content = new VerticalStackLayout
				{
					Padding = 20,
					Spacing = 16,
					Children =
					{
						new Label
						{
							Text = "Toggling IsEnabled should preserve custom native TintColor on toolbar items.",
							AutomationId = "IssueDescriptionLabel"
						},
						toggleButton
					}
				};

#if IOS || MACCATALYST
				Loaded += OnPageLoaded;
#endif
			}

#if IOS || MACCATALYST
			void OnPageLoaded(object sender, EventArgs e)
			{
				Loaded -= OnPageLoaded;

				if (Handler?.PlatformView is UIKit.UIView nativeView)
				{
					var parentViewController = nativeView.Window?.RootViewController;
					var navController = parentViewController as UIKit.UINavigationController
						?? parentViewController?.NavigationController;

					if (navController?.NavigationBar is not null)
					{
						navController.NavigationBar.TintColor = UIKit.UIColor.FromRGB(0, 190, 180);

						var rightItems = navController.VisibleViewController?.NavigationItem?.RightBarButtonItems;
						if (rightItems is not null)
						{
							foreach (var item in rightItems)
							{
								item.TintColor = UIKit.UIColor.FromRGB(255, 45, 146);
							}
						}
					}
				}
			}
#endif
		}
	}
}