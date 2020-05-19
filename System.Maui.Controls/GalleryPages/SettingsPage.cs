using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class SettingsPage : ContentPage
	{
		SettingsScreen _settingsScreen;
		public SettingsPage ()
		{
			_settingsScreen = new SettingsScreen ();
			Content = _settingsScreen;
		}
	}

	public class SettingsScreen : TableView
	{
		public SettingsScreen ()
		{
			Intent = TableIntent.Settings;
			var cell = new TextCell { Text = "Coverflow", Detail = "Value 1" };

			var boolCell = new SwitchCell { Text = "Off" };
			boolCell.OnChanged += (sender, arg) => boolCell.Text = boolCell.On ? "On" : "Off";

			var root = new TableRoot () {
				new TableSection () {
					cell,
					new TextCell { Text = "Cell 2", Detail = "Value 2" },
					new EntryCell {
						Label = "Label",
						Placeholder = "Placeholder 1",
						HorizontalTextAlignment = TextAlignment.Center,
						Keyboard = Keyboard.Numeric
					},
					new ImageCell { Text = "Hello", Detail = "World", ImageSource = "cover1.jpg" }
				},
				new TableSection ("Styles") {
					boolCell,
					new EntryCell {
						Label = "Label2",
						Placeholder = "Placeholder 2",
						HorizontalTextAlignment = TextAlignment.Center,
						Keyboard = Keyboard.Chat
					},
				},
				new TableSection ("Custom Cells") {
					new ViewCell { View = new Button (){ Text = "Hi" } },
				}
			};
			Root = root;
		}
	}
}
