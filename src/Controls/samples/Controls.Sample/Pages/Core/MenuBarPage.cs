using System;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Pages
{
	public partial class MenuBarPage
	{
		public MenuBarPage()
		{
			InitializeComponent();

			this.BindingContext = new MenuBarViewModel();

			
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
			var vm = BindingContext as MenuBarViewModel;
			if (vm != null)
			{
				vm.SwapIcon();
			}
		}
	}
}