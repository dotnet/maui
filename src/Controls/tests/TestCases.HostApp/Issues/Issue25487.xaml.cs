namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25487, "iOS button flickering animation when layout updates", PlatformAffected.iOS)]
	public partial class Issue25487 : ContentPage
	{
		public Issue25487()
		{
			InitializeComponent();
		}

    	bool _isToggled;

		void OnButtonClicked(object sender, EventArgs e)
		{
			if (!_isToggled)
			{
				Button1.Text = "A very long text that will take up more space";
				Button2.Text = "Short";
				Button3.Text = "A very long text that will take up more space";
				Button4.Text = "Short";
			}

			else
			{
				Button1.Text = "Short";
				Button2.Text = "A very long text that will take up more space";
				Button3.Text = "Short";
				Button4.Text = "A very long text that will take up more space";
			}

			_isToggled = !_isToggled;
		}

	}
}