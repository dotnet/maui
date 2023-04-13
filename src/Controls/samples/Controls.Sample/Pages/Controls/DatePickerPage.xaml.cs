using System.Diagnostics;

namespace Maui.Controls.Sample.Pages
{
	public partial class DatePickerPage
	{
		public DatePickerPage()
		{
			InitializeComponent();
		}

		void OnFocusDatePickerFocused(object sender, Microsoft.Maui.Controls.FocusEventArgs e)
		{
			Debug.WriteLine("Focused");
		}

		void OnFocusDatePickerUnfocused(object sender, Microsoft.Maui.Controls.FocusEventArgs e)
		{
			Debug.WriteLine("Unfocused");
		}
	}
}