using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 3701, "Add IsVisible Property to ToolbarItem", PlatformAffected.All)]
	public class Issue3701 : TestContentPage
	{
		protected override void Init()
		{
			var visibleItem = new ToolbarItem("Visible", "", () => { }, ToolbarItemOrder.Primary, 1) { AutomationId = "VisibleItem" };
			var hiddenItem = new ToolbarItem("Hidden", "", () => { }, ToolbarItemOrder.Primary, 2) { AutomationId = "HiddenItem", IsVisible = false };
			var toggleItem = new ToolbarItem("Toggle", "", () => { }, ToolbarItemOrder.Secondary, 1) { AutomationId = "ToggleItem" };
			
			ToolbarItems.Add(visibleItem);
			ToolbarItems.Add(hiddenItem);
			ToolbarItems.Add(toggleItem);

			var toggleVisibleButton = new Button
			{
				Text = "Toggle Hidden Item",
				AutomationId = "ToggleHiddenButton"
			};
			
			var toggleSecondaryButton = new Button
			{
				Text = "Toggle Secondary Item",
				AutomationId = "ToggleSecondaryButton"
			};
			
			var statusLabel = new Label
			{
				Text = "Status: Ready",
				AutomationId = "StatusLabel"
			};

			toggleVisibleButton.Clicked += (sender, e) =>
			{
				hiddenItem.IsVisible = !hiddenItem.IsVisible;
				statusLabel.Text = $"Hidden Item IsVisible: {hiddenItem.IsVisible}";
			};

			toggleSecondaryButton.Clicked += (sender, e) =>
			{
				toggleItem.IsVisible = !toggleItem.IsVisible;
				statusLabel.Text = $"Toggle Item IsVisible: {toggleItem.IsVisible}";
			};

			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "ToolbarItem IsVisible Test Page" },
					statusLabel,
					toggleVisibleButton,
					toggleSecondaryButton,
					new Label { Text = "The 'Hidden' toolbar item should initially be hidden. Use buttons to toggle visibility." }
				},
				Padding = 20
			};
		}
	}
}