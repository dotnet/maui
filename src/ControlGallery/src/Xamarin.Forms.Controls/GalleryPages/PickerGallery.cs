using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class PickerGallery : ContentPage
	{
		#region UpdateMode

		public static readonly BindableProperty UpdateModeProperty = BindableProperty.Create(
			nameof(UpdateMode),
			typeof(UpdateMode),
			typeof(PickerGallery),
			default(UpdateMode));

		public UpdateMode UpdateMode
		{
			get => (UpdateMode)GetValue(UpdateModeProperty);
			set => SetValue(UpdateModeProperty, value);
		}

		#endregion


		public PickerGallery()
		{
			var picker = new Picker { Title = "Dismiss in one sec", Items = { "John", "Paul", "George", "Ringo" } };
			picker.Focused += async (object sender, FocusEventArgs e) =>
			{
				await Task.Delay(1000);
				picker.Unfocus();
			};

			Label testLabel = new Label { Text = "", AutomationId = "test", ClassId = "test" };

			Picker p1 = new Picker
			{
				BindingContext = this,
				Title = "Pick a number",
				Items = { "0", "1", "2", "3", "4", "5", "6" }
			};
			p1.SetBinding(PlatformConfiguration.iOSSpecific.Picker.UpdateModeProperty, UpdateModeProperty.PropertyName);
			p1.SelectedIndexChanged += (sender, e) =>
			{
				testLabel.Text = "Selected Index Changed";
			};
			p1.SetAutomationPropertiesName("Title picker");

			DatePicker datePicker = new DatePicker { BindingContext = this };
			datePicker.SetBinding(PlatformConfiguration.iOSSpecific.DatePicker.UpdateModeProperty, UpdateModeProperty.PropertyName);

			TimePicker timePicker = new TimePicker { BindingContext = this };
			timePicker.SetBinding(PlatformConfiguration.iOSSpecific.TimePicker.UpdateModeProperty, UpdateModeProperty.PropertyName);

			Picker updateModePicker = new EnumPicker
			{
				BindingContext = this,
				Title = "Update Mode",
				EnumType = typeof(UpdateMode)
			};
			updateModePicker.SetBinding(Picker.SelectedItemProperty, UpdateModeProperty.PropertyName, mode: BindingMode.TwoWay);

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Padding = new Thickness(20, 20),
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
						updateModePicker,
						p1,
						datePicker,
						timePicker
					}
				}
			};
		}
	}
}
