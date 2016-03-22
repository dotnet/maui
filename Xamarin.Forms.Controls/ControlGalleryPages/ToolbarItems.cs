using System;

using Xamarin.Forms;

namespace Xamarin.Forms.Controls
{
	public class ToolbarItems : ContentPage
	{
		bool _isEnable = false;
		public ToolbarItems ()
		{
			var label = new Label { Text = "Hello ContentPage", AutomationId ="label_id" };

			var tb1 = new ToolbarItem ("tb1", "menuIcon.png", () => {
				label.Text = "tb1";
			}, ToolbarItemOrder.Primary);
			tb1.IsEnabled = _isEnable;
			tb1.AutomationId = "toolbaritem_primary";
			tb1.IsEnabled = _isEnable;

			var tb2 = new ToolbarItem ("tb2", null, () => {
				label.Text = "tb2";
			}, ToolbarItemOrder.Primary);
			tb2.AutomationId = "toolbaritem_primary2";

			var tb3 = new ToolbarItem ("tb3", "bank.png", () => {
				label.Text = "tb3";
			}, ToolbarItemOrder.Secondary);
			tb3.AutomationId = "toolbaritem_secondary";

			var tb4 = new ToolbarItem ();
			tb4.Text = "tb4";
			tb4.Order = ToolbarItemOrder.Secondary;
			tb4.Command = new Command( (obj)=> {
				_isEnable = true;
				label.Text = "tb4";
				(tb4.Command as Command).ChangeCanExecute();
			},(obj) => _isEnable);
			tb4.AutomationId = "toolbaritem_secondary2";
		
			ToolbarItems.Add(tb1);
			ToolbarItems.Add(tb2);
			ToolbarItems.Add(tb3);
			ToolbarItems.Add(tb4);

			Content = new StackLayout { 
				Children = {
					label
				}
			};
		}
	}
}


