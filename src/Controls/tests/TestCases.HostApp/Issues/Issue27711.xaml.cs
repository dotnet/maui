namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27711, "FlowDirection = `RightToLeft` doesn't work with CV1 & CV2", PlatformAffected.iOS)]
public partial class Issue27711 : ContentPage
{
	public Issue27711()
	{
		InitializeComponent();
	}

	private void switch_Toggled(object sender, ToggledEventArgs e)
	{
		CV.FlowDirection = CV.FlowDirection == FlowDirection.LeftToRight ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
	}
}