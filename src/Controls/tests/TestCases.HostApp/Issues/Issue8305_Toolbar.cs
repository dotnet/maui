using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8305, "ToolbarItem Badge Support", PlatformAffected.All, issueTestNumber: 1)]
public class Issue8305_Toolbar : ContentPage
{
	readonly ToolbarItem _badgeTextItem;
	readonly ToolbarItem _badgeCountItem;
	readonly ToolbarItem _badgeColorItem;
	readonly Label _statusLabel;
	int _count;

	public Issue8305_Toolbar()
	{
		Title = "ToolbarItem Badges";

		_badgeTextItem = new ToolbarItem
		{
			Text = "Mail",
			IconImageSource = "bank.png",
			BadgeText = "New",
			AutomationId = "BadgeTextItem"
		};
		_badgeTextItem.Clicked += (s, e) => _statusLabel.Text = "Tapped: Mail";

		_badgeCountItem = new ToolbarItem
		{
			Text = "Alerts",
			IconImageSource = "calculator.png",
			BadgeText = "3",
			AutomationId = "BadgeCountItem"
		};
		_badgeCountItem.Clicked += (s, e) => _statusLabel.Text = "Tapped: Alerts";

		_badgeColorItem = new ToolbarItem
		{
			Text = "Cart",
			IconImageSource = "shopping_cart.png",
			BadgeText = "1",
			BadgeColor = Colors.Green,
			AutomationId = "BadgeColorItem"
		};
		_badgeColorItem.Clicked += (s, e) => _statusLabel.Text = "Tapped: Cart";

		ToolbarItems.Add(_badgeTextItem);
		ToolbarItems.Add(_badgeCountItem);
		ToolbarItems.Add(_badgeColorItem);

		_statusLabel = new Label
		{
			Text = "Tap toolbar items or use buttons below",
			AutomationId = "StatusLabel"
		};

		_count = 3;

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = 20,
				Children =
				{
					_statusLabel,
					new Button
					{
						Text = "Increment Count Badge",
						AutomationId = "IncrementButton",
						Command = new Command(() =>
						{
							_count++;
							_badgeCountItem.BadgeText = _count.ToString();
							_statusLabel.Text = $"Count badge: {_count}";
						})
					},
					new Button
					{
						Text = "Clear All Badges",
						AutomationId = "ClearBadgesButton",
						Command = new Command(() =>
						{
							_badgeTextItem.BadgeText = null;
							_badgeCountItem.BadgeText = null;
							_badgeColorItem.BadgeText = null;
							_statusLabel.Text = "All badges cleared";
						})
					},
					new Button
					{
						Text = "Set Badge Color Red",
						AutomationId = "SetRedColorButton",
						Command = new Command(() =>
						{
							_badgeColorItem.BadgeColor = Colors.Red;
							_statusLabel.Text = "Badge color: Red";
						})
					},
					new Button
					{
						Text = "Restore Badges",
						AutomationId = "RestoreBadgesButton",
						Command = new Command(() =>
						{
							_count = 3;
							_badgeTextItem.BadgeText = "New";
							_badgeCountItem.BadgeText = "3";
							_badgeColorItem.BadgeText = "1";
							_badgeColorItem.BadgeColor = Colors.Green;
							_statusLabel.Text = "Badges restored";
						})
					}
				}
			}
		};
	}
}
