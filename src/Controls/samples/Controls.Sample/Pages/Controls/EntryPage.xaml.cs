using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class EntryPage
	{
		public EntryPage()
		{
			InitializeComponent();
			entry.PropertyChanged += Entry_PropertyChanged;
		}

		private void Entry_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(entry.CursorPosition))
				lblCursor.Text = $"CursorPosition = {entry.CursorPosition}";
		}

		void OnEntryCompleted(object sender, EventArgs e)
		{
			var text = ((Entry)sender).Text;
			DisplayAlert("Completed", text, "Ok");
		}
	}
}