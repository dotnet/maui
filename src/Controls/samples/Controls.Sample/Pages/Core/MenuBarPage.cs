using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

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

		void ToggleMenuBarItem(object sender, EventArgs e)
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

				MenuBarItems.Add(barItem);
			}
			else
			{
				MenuBarItems.Remove(barItem);
			}
		}
	}
}