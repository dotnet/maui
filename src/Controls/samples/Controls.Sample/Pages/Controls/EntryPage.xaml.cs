using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class EntryPage
	{
		public EntryPage()
		{
			InitializeComponent();
		}

		void OnEntryCompleted(object sender, EventArgs e)
		{
			var text = ((Entry)sender).Text;
			DisplayAlert("Completed", text, "Ok");
		}
	}
}