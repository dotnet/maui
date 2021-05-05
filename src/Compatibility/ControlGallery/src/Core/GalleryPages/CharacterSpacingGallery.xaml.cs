using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CharacterSpacingGallery : ContentPage
	{
		public CharacterSpacingGallery()
		{
			InitializeComponent();
			textColorPicker.InitWithColor(Colors.Red);
			placeholderColorPicker.InitWithColor(Colors.BlueViolet);
		}

		void Slider_OnValueChanged(object sender, ValueChangedEventArgs e)
		{
			CharacterSpacingValue.Text = e.NewValue.ToString();
			Button.CharacterSpacing = e.NewValue;
			DatePicker.CharacterSpacing = e.NewValue;
			Editor.CharacterSpacing = e.NewValue;
			Entry.CharacterSpacing = e.NewValue;
			PlaceholderEntry.CharacterSpacing = e.NewValue;
			PlaceholderEditor.CharacterSpacing = e.NewValue;
			Label.CharacterSpacing = e.NewValue;
			Picker.CharacterSpacing = e.NewValue;
			SearchBar.CharacterSpacing = e.NewValue;
			PlaceholderSearchBar.CharacterSpacing = e.NewValue;
			TimePicker.CharacterSpacing = e.NewValue;
			Span.CharacterSpacing = e.NewValue;
		}

		void ColorPicker_OnColorPicked(object sender, ColorPickedEventArgs e)
		{
			if (sender == textColorPicker)
			{
				Button.TextColor = e.Color;
				DatePicker.TextColor = e.Color;
				Editor.TextColor = e.Color;
				Entry.TextColor = e.Color;
				PlaceholderEntry.TextColor = e.Color;
				PlaceholderEditor.TextColor = e.Color;
				Label.TextColor = e.Color;
				Picker.TextColor = e.Color;
				SearchBar.TextColor = e.Color;
				PlaceholderSearchBar.TextColor = e.Color;
				TimePicker.TextColor = e.Color;
				Span.TextColor = e.Color;
			}
			else
			{
				PlaceholderEntry.PlaceholderColor = e.Color;
				PlaceholderEditor.PlaceholderColor = e.Color;
				PlaceholderSearchBar.PlaceholderColor = e.Color;

			}
		}

		void ResetButtonClicked(object sender, EventArgs e)
		{
			slider.Value = 0;
			textColorPicker.InitWithColor(Colors.Red);
			placeholderColorPicker.InitWithColor(Colors.BlueViolet);
		}
	}
}