using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class EntryPageAndroid : ContentPage
	{
		Label _lbl;
		Entry _entry;
		Picker _picker;
		public EntryPageAndroid()
		{
			_entry = new Entry
			{
				FontSize = 22,
				Placeholder = "Type and use the picker to set your ImeFlags"
			};

			_entry.On<Android>().SetImeOptions(ImeFlags.Default);

			_lbl = new Label
			{
				FontSize = 20
			};

			_picker = new Picker();
			_picker.Items.Add(ImeFlags.Default.ToString());
			_picker.Items.Add(ImeFlags.Go.ToString());
			_picker.Items.Add(ImeFlags.Next.ToString());
			_picker.Items.Add(ImeFlags.Previous.ToString());
			_picker.Items.Add(ImeFlags.Search.ToString());
			_picker.Items.Add(ImeFlags.Send.ToString());
			_picker.Items.Add(ImeFlags.Done.ToString());
			_picker.Items.Add(ImeFlags.NoAccessoryAction.ToString());
			_picker.Items.Add(ImeFlags.None.ToString());
			_picker.Items.Add(ImeFlags.NoExtractUi.ToString());
			_picker.Items.Add(ImeFlags.NoPersonalizedLearning.ToString());
			_picker.Items.Add(ImeFlags.NoFullscreen.ToString());
			_picker.SelectedIndexChanged += _picker_SelectedIndexChanged;
			_picker.SelectedIndex = 0;
			Content = new StackLayout
			{
				Children =
				{
					_lbl,
					_entry,
					_picker
				}
			};
		}

		void _picker_SelectedIndexChanged(object sender, EventArgs e)
		{
			ImeFlags flag = (ImeFlags)Enum.Parse(typeof(ImeFlags), _picker.SelectedItem.ToString());
			_entry.On<Android>().SetImeOptions(flag);
			UpdateLabelText();
		}

		private void UpdateLabelText()
		{
			_lbl.Text = $"Default ImeOptions {_entry.On<Android>().ImeOptions()}";
		}

	}
}
