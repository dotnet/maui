using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18797, "[Android] Datepicker focus and unfocus event not firing on android", PlatformAffected.Android)]
	public partial class Issue18797 : ContentPage
	{
		public Issue18797()
		{
			InitializeComponent();
		}

		void DatePicker_Focused(System.Object sender, Microsoft.Maui.Controls.FocusEventArgs e)
		{
			FocusedLabel.Text = "Focused: true";
		}

		void DatePicker_Unfocused(System.Object sender, Microsoft.Maui.Controls.FocusEventArgs e)
		{
			UnfocusedLabel.Text = "Unfocused: true";
		}
	}
}