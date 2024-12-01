namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 23119, "Assigning a Brush of type RadialGradientBrush to the Background property of an ImageButton causes the BG to show a solid color", PlatformAffected.Android)]
	public partial class Issue23119 : ContentPage
	{
		public Issue23119()
		{
			InitializeComponent();
		}
	}
}