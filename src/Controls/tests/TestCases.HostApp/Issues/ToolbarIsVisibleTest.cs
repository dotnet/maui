using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 0, "Toolbar IsVisible Property Test", PlatformAffected.All)]
	public class ToolbarIsVisibleTest : ContentPage
	{
		public ToolbarIsVisibleTest()
		{
			Title = "Toolbar IsVisible Test";

			var visibleItem = new ToolbarItem
			{
				Text = "Visible",
				IconImageSource = "dotnet_bot.png",
				AutomationId = "VisibleToolbarItem"
			};

			var hiddenItem = new ToolbarItem
			{
				Text = "Hidden",
				IconImageSource = "dotnet_bot.png",
				IsVisible = false,
				AutomationId = "HiddenToolbarItem"
			};

			var toggleItem = new ToolbarItem
			{
				Text = "Toggle",
				IconImageSource = "dotnet_bot.png",
				AutomationId = "ToggleToolbarItem"
			};

			var secondaryVisibleItem = new ToolbarItem
			{
				Text = "Secondary Visible",
				Order = ToolbarItemOrder.Secondary,
				AutomationId = "SecondaryVisibleToolbarItem"
			};

			var secondaryHiddenItem = new ToolbarItem
			{
				Text = "Secondary Hidden",
				Order = ToolbarItemOrder.Secondary,
				IsVisible = false,
				AutomationId = "SecondaryHiddenToolbarItem"
			};

			ToolbarItems.Add(visibleItem);
			ToolbarItems.Add(hiddenItem);
			ToolbarItems.Add(toggleItem);
			ToolbarItems.Add(secondaryVisibleItem);
			ToolbarItems.Add(secondaryHiddenItem);

			var toggleVisibilityButton = new Button
			{
				Text = "Toggle Hidden Item Visibility",
				AutomationId = "ToggleHiddenButton"
			};

			var toggleAllButton = new Button
			{
				Text = "Toggle All Items Visibility",
				AutomationId = "ToggleAllButton"
			};

			var toggleSecondaryButton = new Button
			{
				Text = "Toggle Secondary Hidden Item",
				AutomationId = "ToggleSecondaryButton"
			};

			var statusLabel = new Label
			{
				Text = "Status: Initial state",
				AutomationId = "StatusLabel"
			};

			var hiddenItemStatusLabel = new Label
			{
				Text = $"Hidden Item Visible: {hiddenItem.IsVisible}",
				AutomationId = "HiddenItemStatusLabel"
			};

			var secondaryHiddenStatusLabel = new Label
			{
				Text = $"Secondary Hidden Item Visible: {secondaryHiddenItem.IsVisible}",
				AutomationId = "SecondaryHiddenStatusLabel"
			};

			toggleVisibilityButton.Clicked += (sender, e) =>
			{
				hiddenItem.IsVisible = !hiddenItem.IsVisible;
				hiddenItemStatusLabel.Text = $"Hidden Item Visible: {hiddenItem.IsVisible}";
				statusLabel.Text = $"Status: Hidden item is now {(hiddenItem.IsVisible ? "visible" : "hidden")}";
			};

			toggleAllButton.Clicked += (sender, e) =>
			{
				var newVisibility = !visibleItem.IsVisible;
				visibleItem.IsVisible = newVisibility;
				hiddenItem.IsVisible = newVisibility;
				toggleItem.IsVisible = newVisibility;
				
				hiddenItemStatusLabel.Text = $"Hidden Item Visible: {hiddenItem.IsVisible}";
				statusLabel.Text = $"Status: All items are now {(newVisibility ? "visible" : "hidden")}";
			};

			toggleSecondaryButton.Clicked += (sender, e) =>
			{
				secondaryHiddenItem.IsVisible = !secondaryHiddenItem.IsVisible;
				secondaryHiddenStatusLabel.Text = $"Secondary Hidden Item Visible: {secondaryHiddenItem.IsVisible}";
				statusLabel.Text = $"Status: Secondary hidden item is now {(secondaryHiddenItem.IsVisible ? "visible" : "hidden")}";
			};

			// Handle toolbar item clicks
			visibleItem.Clicked += (sender, e) =>
			{
				statusLabel.Text = "Status: Visible item clicked";
			};

			hiddenItem.Clicked += (sender, e) =>
			{
				statusLabel.Text = "Status: Hidden item clicked";
			};

			toggleItem.Clicked += (sender, e) =>
			{
				statusLabel.Text = "Status: Toggle item clicked";
			};

			secondaryVisibleItem.Clicked += (sender, e) =>
			{
				statusLabel.Text = "Status: Secondary visible item clicked";
			};

			secondaryHiddenItem.Clicked += (sender, e) =>
			{
				statusLabel.Text = "Status: Secondary hidden item clicked";
			};

			Content = new StackLayout
			{
				Padding = 20,
				Children =
				{
					new Label
					{
						Text = "This page demonstrates the ToolbarItem IsVisible property.",
						FontSize = 16,
						HorizontalOptions = LayoutOptions.Center
					},
					new Label
					{
						Text = "• 'Visible' item should always be visible\n• 'Hidden' item starts hidden\n• 'Toggle' item can be used to test interactions\n• Secondary items appear in overflow menu",
						FontSize = 14,
						Margin = new Thickness(0, 10)
					},
					statusLabel,
					hiddenItemStatusLabel,
					secondaryHiddenStatusLabel,
					toggleVisibilityButton,
					toggleAllButton,
					toggleSecondaryButton
				}
			};
		}
	}
}