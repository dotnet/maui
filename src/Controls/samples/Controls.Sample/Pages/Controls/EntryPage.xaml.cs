using System;
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Entry = Microsoft.Maui.Controls.Entry;

namespace Maui.Controls.Sample.Pages
{
	public class TransparentEntry : Entry
	{

	}

	public partial class EntryPage
	{
		public EntryPage()
		{
			InitializeComponent();

			entryCursor.PropertyChanged += OnEntryPropertyChanged;
			entrySelection.PropertyChanged += OnEntrySelectionPropertyChanged;

			sldSelection.Maximum = entrySelection.Text.Length;
			sldSelection.Value = entrySelection.SelectionLength;
			sldCursorPosition.Maximum = entryCursor.Text.Length;
			sldCursorPosition.Value = entryCursor.CursorPosition;

			PlatformSpecificEntry.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
				.SetImeOptions(ImeFlags.Search);

			UpdateEntryBackground();
			UpdateEntryBackgroundColor();
		}

		void OnSlideCursorPositionValueChanged(object sender, ValueChangedEventArgs e)
		{
			entryCursor.CursorPosition = (int)e.NewValue;
		}

		void OnSlideSelectionValueChanged(object sender, ValueChangedEventArgs e)
		{
			entrySelection.SelectionLength = (int)e.NewValue;
		}

		void OnEntryPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Entry.CursorPosition))
				lblCursor.Text = $"CursorPosition = {((Entry)sender).CursorPosition}";
			if (e.PropertyName == nameof(Entry.Text))
				sldCursorPosition.Maximum = ((Entry)sender).Text.Length;
		}

		void OnEntrySelectionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Entry.SelectionLength))
				lblSelection.Text = $"SelectionLength = {((Entry)sender).SelectionLength}";
			if (e.PropertyName == nameof(Entry.Text))
				sldSelection.Maximum = ((Entry)sender).Text.Length;
		}

		void OnEntryCompleted(object sender, EventArgs e)
		{
			var text = ((Microsoft.Maui.Controls.Entry)sender).Text;
			DisplayAlert("Completed", text, "Ok");
		}

		void OnEntryFocused(object sender, FocusEventArgs e)
		{
			var text = ((Microsoft.Maui.Controls.Entry)sender).Text;
			DisplayAlert("Focused", text, "Ok");
		}

		void OnEntryUnfocused(object sender, FocusEventArgs e)
		{
			var text = ((Entry)sender).Text;
			DisplayAlert("Unfocused", text, "Ok");
		}

		void OnUpdateBackgroundColorButtonClicked(object sender, System.EventArgs e)
		{
			UpdateEntryBackgroundColor();
		}

		void OnClearBackgroundColorButtonClicked(object sender, System.EventArgs e)
		{
			BackgroundColorEntry.BackgroundColor = null;
		}

		void OnUpdateBackgroundButtonClicked(object sender, System.EventArgs e)
		{
			UpdateEntryBackground();
		}

		void OnClearBackgroundButtonClicked(object sender, System.EventArgs e)
		{
			BackgroundEntry.Background = null;
		}

		void UpdateEntryBackgroundColor()
		{
			Random rnd = new Random();
			Color backgroundColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			BackgroundColorEntry.BackgroundColor = backgroundColor;
		}

		void UpdateEntryBackground()
		{
			Random rnd = new Random();
			Color startColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			Color endColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);

			BackgroundEntry.Background = new LinearGradientBrush
			{
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = startColor },
					new GradientStop { Color = endColor, Offset = 1 }
				}
			};
		}

		void ShowSoftInputAsyncButton_Clicked(object sender, EventArgs e)
		{
			PlaceholderEntryItem.ShowSoftInputAsync(System.Threading.CancellationToken.None);
		}

		void HideSoftInputAsyncButton_Clicked(object sender, EventArgs e)
		{
			PlaceholderEntryItem.HideSoftInputAsync(System.Threading.CancellationToken.None);
		}

		void OnReturnTypeEntryTextChanged(object sender, TextChangedEventArgs e)
		{
			Random rnd = new Random();
			var returnTypeCount = Enum.GetNames(typeof(ReturnType)).Length;
			ReturnTypeEntry.ReturnType = (ReturnType)rnd.Next(0, returnTypeCount);

			Debug.WriteLine($"ReturnType: {ReturnTypeEntry.ReturnType}");
		}
	}
}