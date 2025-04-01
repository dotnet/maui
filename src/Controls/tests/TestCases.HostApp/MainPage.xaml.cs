namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private void LoginButton_Clicked(object sender, EventArgs e)
		{
			UserName.Text = string.Empty;
			Password.Text = string.Empty;
			StatusLabel.Text = "Logging in " + DateTime.Now.ToString("HH:mm:ss");
		}
	}
}