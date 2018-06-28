using System;

using Xamarin.Forms;

namespace Xamarin.Forms.Controls
{
	public class ToolbarItems : ContentPage
	{
		bool _isEnable = false;
		public ToolbarItems()
		{

			var label = new Label { Text = "Hello ContentPage", AutomationId = "label_id" };

			var command = new Command((obj) =>
							{
								label.Text = "button 4 new text";
							}, (obj) => _isEnable);
			var tb1 = new ToolbarItem("tb1", "menuIcon.png", () =>
			{
				label.Text = "tb1";
			}, ToolbarItemOrder.Primary);
			tb1.IsEnabled = _isEnable;
			tb1.AutomationId = "toolbaritem_primary";

			var tb2 = new ToolbarItem("tb2", null, () =>
			{
				label.Text = "tb2";
			}, ToolbarItemOrder.Primary);
			tb2.AutomationId = "toolbaritem_primary2";

			var tb3 = new ToolbarItem("tb3", "bank.png", () =>
			{
				label.Text = "tb3";
				_isEnable = !_isEnable;
				command.ChangeCanExecute();
			}, ToolbarItemOrder.Secondary);
			tb3.AutomationId = "toolbaritem_secondary";

			var tb4 = new ToolbarItem();
			tb4.Text = "tb4";
			tb4.Order = ToolbarItemOrder.Secondary;
			tb4.Command = command;
			tb4.Icon = "coffee";
			tb4.AutomationId = "toolbaritem_secondary2";

			var tb5 = new ToolbarItem();
			tb5.Text = "tb5";
			tb5.Icon = "bank.png";
			tb5.Order = ToolbarItemOrder.Secondary;
			tb5.Command = new Command(async () => {
				await Navigation.PushAsync(new ToolbarItems());
			});
			tb5.AutomationId = "toolbaritem_secondary5";

			ToolbarItems.Add(tb1);
			ToolbarItems.Add(tb2);
			ToolbarItems.Add(tb3);
			ToolbarItems.Add(tb4);
			ToolbarItems.Add(tb5);

			Content = new StackLayout
			{
				Children = {
					label
				}
			};
		}
	}
}


