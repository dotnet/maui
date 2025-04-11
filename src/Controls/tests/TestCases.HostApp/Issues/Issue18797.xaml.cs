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

		void DatePicker_Focused(object sender, FocusEventArgs e)
		{
			FocusedLabel.Text = "Focused: true";
		}

		void DatePicker_Unfocused(object sender, FocusEventArgs e)
		{
			UnfocusedLabel.Text = "Unfocused: true";
		}
	}
}