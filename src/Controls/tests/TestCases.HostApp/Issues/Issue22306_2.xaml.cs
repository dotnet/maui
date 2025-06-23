namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22306_2, "Button sizes content with respect to the BorderWidth", PlatformAffected.iOS)]
	public partial class Issue22306_2 : ContentPage
	{
		bool toggleBorderWidth = false;

		public Issue22306_2()
		{
			InitializeComponent();
		}

		void Change_BorderWidth_Button_Clicked(object sender, System.EventArgs e)
		{
			toggleBorderWidth = !toggleBorderWidth;

			TextButton.BorderWidth = toggleBorderWidth ? 5 : 0;
			ImageButton.BorderWidth = toggleBorderWidth ? 20 : 0;
			TextImageButton.BorderWidth = toggleBorderWidth ? 20 : 0;
		}
	}
}
