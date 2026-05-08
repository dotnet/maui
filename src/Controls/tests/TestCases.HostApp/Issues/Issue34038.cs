namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34038, "[macOS] IsEnabled property false not working on MenuBarItem", PlatformAffected.macOS | PlatformAffected.UWP)]
public class Issue34038 : TestShell
{
	const string InitialStatus = "Failure";

	MenuBarItem _menuBarItem;
	Label _statusLabel;

	protected override void Init()
	{
		_menuBarItem = new MenuBarItem
		{
			Text = "Issue34038MenuBarItem",
			IsEnabled = false
		};

		var flyoutItem = new MenuFlyoutItem
		{
			Text = "Issue34038MenuFlyoutItem"
		};

		flyoutItem.Clicked += (_, _) => _statusLabel.Text = "Success";
		_menuBarItem.Add(flyoutItem);
		MenuBarItems.Add(_menuBarItem);

		_statusLabel = new Label
		{
			AutomationId = "Issue34038StatusLabel",
			Text = InitialStatus
		};

		var isEnabledSwitch = new Switch
		{
			AutomationId = "Issue34038MenuEnabledSwitch",
			IsToggled = false
		};

		_menuBarItem.IsEnabled = isEnabledSwitch.IsToggled;
		isEnabledSwitch.Toggled += (_, e) => _menuBarItem.IsEnabled = e.Value;

		var instructions = new Label
		{
			Text = "Toggle the switch off and verify the menu bar item cannot be opened.",
			LineBreakMode = LineBreakMode.WordWrap
		};

		AddContentPage(new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 12,
				Children =
				{
					new Label { Text = "MenuBarItem IsEnabled regression test" },
					instructions,
					new HorizontalStackLayout
					{
						Spacing = 10,
						Children =
						{
							new Label { Text = "MenuBarItem IsEnabled" },
							isEnabledSwitch
						}
					},
					_statusLabel
				}
			}
		});
	}
}