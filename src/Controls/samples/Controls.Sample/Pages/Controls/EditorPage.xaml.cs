using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class EditorPage
	{
		public EditorPage()
		{
			InitializeComponent();
		}

		void OnEditorCompleted(object sender, EventArgs e)
		{
			var text = ((Editor)sender).Text;
			DisplayAlert("Completed", text, "Ok");
		}

		void OnEditorFocused(object sender, FocusEventArgs e)
		{
			var text = ((Editor)sender).Text;
			DisplayAlert("Focused", text, "Ok");
		}

		void OnEditorUnfocused(object sender, FocusEventArgs e)
		{
			var text = ((Editor)sender).Text;
			DisplayAlert("Unfocused", text, "Ok");
		}
	}
}