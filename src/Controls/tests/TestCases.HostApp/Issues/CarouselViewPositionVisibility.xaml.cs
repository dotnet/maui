namespace Maui.Controls.Sample.Issues
{
	// Issue12848 (src\ControlGallery\src\Issues.Shared\Issue12848.cs
	[Issue(IssueTracker.Github, 12848, "[Bug] CarouselView position resets when visibility toggled", PlatformAffected.Android)]
	public partial class CarouselViewPositionVisibility : ContentPage
	{
		public CarouselViewPositionVisibility()
		{
			InitializeComponent();

			BindingContext = new List<int> { 1, 2, 3 };
		}

		void OnShowButtonClicked(object sender, EventArgs e)
		{
			carousel.IsVisible = true;
		}

		void OnHideButtonClicked(object sender, EventArgs e)
		{
			carousel.IsVisible = false;
		}
	}
}