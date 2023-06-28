using System;
using System.Linq;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class MenuBarPage
	{
		public MenuBarPage()
		{
			InitializeComponent();
		}

		void ItemClicked(object sender, EventArgs e)
		{
			if (sender is MenuFlyoutItem mfi)
			{
				menuLabel.Text = $"You clicked on Menu Item: {mfi.Text}";
			}
		}

		void OnToggleMenuBarItem(object sender, EventArgs e)
		{
			MenuBarItem barItem =
				MenuBarItems.FirstOrDefault(x => x.Text == "Added Menu");

			if (barItem == null)
			{
				barItem = new MenuBarItem()
				{
					Text = "Added Menu"
				};

				barItem.Add(new MenuFlyoutItem()
				{
					Text = "Added Flyout Item",
					Command = new Command(() => ItemClicked(barItem.First(), EventArgs.Empty))
				});

				barItem.Add(new MenuFlyoutItem()
				{
					Text = "Added Disabled Flyout Item",
					IsEnabled = false,
					Command = new Command(() => ItemClicked(barItem.First(), EventArgs.Empty))
				});

				MenuBarItems.Add(barItem);
			}
			else
			{
				MenuBarItems.Remove(barItem);
			}
		}
	}
}