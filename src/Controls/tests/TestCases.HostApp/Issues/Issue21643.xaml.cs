using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21643, "Border has an unexpected background animation", PlatformAffected.iOS)]

public partial class Issue21643 : ContentPage
{
	private bool isBorderOn = false;

	public Issue21643()
	{
		InitializeComponent();
	}

	private void OnBorderChanged(object sender, EventArgs e)
	{
		if (this.isBorderOn)
		{
			this.TheBorder.BackgroundColor = Colors.Red;
			this.TheBorder.StrokeShape = new Rectangle();

		}
		else
		{
			this.TheBorder.BackgroundColor = Colors.Green;
			this.TheBorder.StrokeShape = new RoundRectangle { CornerRadius = new(100.0) };
		}

		this.isBorderOn = !this.isBorderOn;
	}
}
