#if IOS || MACCATALYST
using Microsoft.Maui.Controls.Handlers.Compatibility;
using UIKit;
#endif

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 39061, "ToolbarItem custom TintColor is lost when IsEnabled changes on iOS", PlatformAffected.iOS | PlatformAffected.macOS)]
	public class Issue39061 : NavigationPage
	{
		public Issue39061() : base(new MainPage())
		{
		}

		class MainPage : ContentPage
		{
			readonly ToolbarItem _saveItem;
			readonly ToolbarItem _refreshItem;

			public MainPage()
			{
				Title = "Issue 39061";

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
			}
		}
	}

#if IOS || MACCATALYST
	public sealed class Issue39061NavigationRenderer : NavigationRenderer
	{
		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			NavigationBar.TintColor = UIColor.FromRGB(0, 190, 180);

			var rightBarButtonItems = VisibleViewController?.NavigationItem.RightBarButtonItems;
			if (rightBarButtonItems is null)
				return;

			foreach (var item in rightBarButtonItems)
				item.TintColor = UIColor.FromRGB(255, 45, 146);
		}
	}
#endif
}