using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
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

			var fis = new FontImageSource()
			{
				FontFamily = GetFontFamily(),
				Glyph = '\uf101'.ToString(),
				Color = Colors.Red
			};

			var tb2 = new ToolbarItem("tb2 font", null, () =>
			{
				label.Text = "tb2";
			}, ToolbarItemOrder.Primary);
			tb2.IconImageSource = fis;
			tb2.AutomationId = "toolbaritem_primary2";
			var tb6 = new ToolbarItem("tb6 long long text", null, () =>
			{
				label.Text = "tb6";
			}, ToolbarItemOrder.Primary);
			tb6.AutomationId = "toolbaritem_primary6";

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
			tb4.IconImageSource = "coffee";
			tb4.AutomationId = "toolbaritem_secondary2";

			var tb5 = new ToolbarItem();
			tb5.Text = "tb5";
			tb5.IconImageSource = "bank.png";
			tb5.Order = ToolbarItemOrder.Secondary;
			tb5.Command = new Command(async () =>
			{
				await Navigation.PushAsync(new ToolbarItems());
			});
			tb5.AutomationId = "toolbaritem_secondary5";

			ToolbarItems.Add(tb1);
			ToolbarItems.Add(tb2);
			ToolbarItems.Add(tb3);
			ToolbarItems.Add(tb4);
			ToolbarItems.Add(tb5);
			ToolbarItems.Add(tb6);

			Content = new StackLayout
			{
				Children = {
					label
				}
			};
		}

		static string GetFontFamily()
		{
			var fontFamily = "";
			switch (Device.RuntimePlatform)
			{
				case Device.macOS:
				case Device.iOS:
					fontFamily = "Ionicons";
					break;
				case Device.UWP:
					fontFamily = "Assets/Fonts/ionicons.ttf#ionicons";
					break;
				case Device.WPF:
				case Device.GTK:
					fontFamily = "Assets/ionicons.ttf#ionicons";
					break;
				case Device.Android:
				default:
					fontFamily = "fonts/ionicons.ttf#";
					break;
			}

			return fontFamily;
		}
	}
}


