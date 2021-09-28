using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class EntryPage
	{
		public EntryPage()
		{
			InitializeComponent();
			entryCursor.PropertyChanged += OnEntryPropertyChanged;
		}

		void OnEntryPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Entry.CursorPosition))
				lblCursor.Text = $"CursorPosition = {((Entry)sender).CursorPosition}";
		}

		void OnEntryCompleted(object sender, EventArgs e)
		{
			var text = ((Entry)sender).Text;
			DisplayAlert("Completed", text, "Ok");
		}
	}
}