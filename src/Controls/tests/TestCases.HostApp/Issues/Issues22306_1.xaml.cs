namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22306_1, "Button with images measures correctly", PlatformAffected.iOS)]
	public partial class Issues22306_1 : ContentPage
	{
		public Issues22306_1()
		{
			InitializeComponent();
		}

		void RotateButton_Clicked(object sender, System.EventArgs e)
		{
			var button = (Button)sender;
			var newPosition = ((int)button.ContentLayout.Position + 1) % 4;

			var contentLayout =
				new Button.ButtonContentLayout((Button.ButtonContentLayout.ImagePosition)newPosition,
					button.ContentLayout.Spacing);

			button.ContentLayout = contentLayout;
			ConstrainedButton.ContentLayout = contentLayout;
		}
	}
}
