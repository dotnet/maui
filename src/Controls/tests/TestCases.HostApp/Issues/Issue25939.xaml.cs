namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25939, "Unable to Remove Thumb Image in Slider Control ", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue25939 : ContentPage
	{
		public Issue25939()
		{
			InitializeComponent();
		}

		private void OnThumbImageSourceButtonClicked(object sender, EventArgs e)
		{
			SliderControl.ThumbImageSource = "coffee.png";
			SliderControl2.ThumbImageSource = "coffee.png";
		}
		private void OnResetButtonClicked(object sender, EventArgs e)
		{
			SliderControl2.ThumbImageSource = null;
		}
	}
}