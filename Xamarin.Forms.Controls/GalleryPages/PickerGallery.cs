using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class PickerGallery : ContentPage
	{
		public PickerGallery ()
		{
			var picker = new Picker {Title="Dismiss in one sec", Items =  {"John", "Paul", "George", "Ringo"}};
			picker.Focused += async (object sender, FocusEventArgs e) => {
				await Task.Delay (1000);
				picker.Unfocus ();
			};

			Label testLabel = new Label { Text = "", AutomationId="test", ClassId="test" };

			Picker p1 = new Picker { Title = "Pick a number", Items = { "0", "1", "2", "3", "4", "5", "6" }};
			p1.SelectedIndexChanged += (sender, e) => {
				testLabel.Text = "Selected Index Changed";
			};

			Content = new ScrollView { 
				Content = new StackLayout {
					Padding = new Thickness (20, 20),
					Children = {
						new DatePicker (),
						new TimePicker (),
						new DatePicker { Format = "D" },
						new TimePicker { Format = "T" },
						new Picker {Title = "Set your favorite Beatle", Items =  {"John", "Paul", "George", "Ringo"}},
						new Picker {Title = "Set your favorite Stone", Items = {"Mick", "Keith", "Charlie", "Ronnie"}, SelectedIndex = 1},
						new Picker {Title = "Pick", Items =  {"Jason Smith", "Rui Marinho", "Eric Maupin", "Chris King"}, HorizontalOptions = LayoutOptions.CenterAndExpand},
						picker,
						testLabel,
						p1
					}
				}
			};
		}
	}
}
