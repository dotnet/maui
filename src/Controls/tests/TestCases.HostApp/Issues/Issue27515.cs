namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27515, "Slider's thumb image doesn't respect color", PlatformAffected.iOS)]
	public class Issue27515 : ContentPage
	{
		public Issue27515()
		{
			var slider = new Slider() { ThumbColor = Colors.Blue };
			Content = new VerticalStackLayout()
			{
				slider,
				new Button()
				{
					AutomationId = "ChangeThumbImageButton",
					Text = "Click to change image",
					Command = new Command(() => {
						slider.ThumbImageSource = "coffee.png";
					})
				},
				new Button()
				{
					AutomationId = "ChangeThumbColorButton",
					Text = "Click to change color",
					Command = new Command(() => {
						slider.ThumbColor = Colors.Red;
					})
				}
			};
		}
	}
}