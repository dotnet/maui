namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26856, "MenuFlyoutItems programmatically added to MenuFlyoutSubItems are not visible", PlatformAffected.UWP)]
	public class Issue26856 : TestShell
	{
		protected override void Init()
		{
			// Create MenuBarItem
			var menuBarItem = new MenuBarItem { Text = "Menu Flyout Item" };

			// Add MenuFlyoutItem to MenuBarItem
			menuBarItem.Add(new MenuFlyoutItem { Text = "Original Item" });

			// Create MenuFlyoutSubItem
			var menuFlyoutSubItem = new MenuFlyoutSubItem { Text = "Flyout" };

			// Add MenuFlyoutItem to MenuFlyoutSubItem
			menuFlyoutSubItem.Add(new MenuFlyoutItem { Text = "Original Sub Item" });

			// Add MenuFlyoutSubItem to MenuBarItem
			menuBarItem.Add(menuFlyoutSubItem);

			// Add MenuBarItem to MenuBarItems
			MenuBarItems.Add(menuBarItem);

			var layout = new VerticalStackLayout
			{
				Padding = new Thickness(30, 0),
				Spacing = 25
			};

			var label = new Label { Text = "Menu Test" };
			var button = new Button
			{
				Text = "Add Flyout Sub Item"
			};
			button.AutomationId = "Button";

			button.Clicked += OnButtonClicked;

			// Add elements to layout
			layout.Children.Add(label);
			layout.Children.Add(button);

			// Set Content of the Page
			AddContentPage(new ContentPage { Content = layout });
			FlyoutBehavior = FlyoutBehavior.Disabled;
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			MenuFlyoutSubItem menuFlyoutSubItem = GetSubMenu("Flyout");

			MenuFlyoutItem itemToAdd = new()
			{
				Text = "Added Sub Item",
				IsEnabled = true,
				Parent = menuFlyoutSubItem
			};
			menuFlyoutSubItem.Add(itemToAdd);

		}

		public MenuFlyoutSubItem GetSubMenu(string name)
		{
			MenuFlyoutSubItem result = null;

			MenuBarItems.ToList().ForEach(menuBarItem =>
			{
				var foundItem = menuBarItem.SingleOrDefault(menuElement => menuElement is MenuFlyoutSubItem subMenu && subMenu.Text == name);

				if (foundItem != null)
				{
					result = foundItem as MenuFlyoutSubItem;
				}
			});

			return result;
		}
	}
}