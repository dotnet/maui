using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24414, "Shadows not rendering as expected on Android and iOS", PlatformAffected.Android | PlatformAffected.iOS)]

public partial class Issue24414 : ContentPage
{
	int fn = 1;
	string[] labels = ["HELLO WORLD", "LLOHE WODRL", "OLLEH ORLWD", "LOHEL LODRW"];

	public Issue24414()
	{
		InitializeComponent();
		UpdateLabel();
	}

	private void OnTapGestureRecognizerTapped(object sender, EventArgs e)
	{
		var grid = (Grid)Content;
		foreach (IView view in grid)
		{
			if (view is Border { Shadow: not null } border)
			{
				switch (fn)
				{
					case 1:
						border.WidthRequest += 4;
						border.HeightRequest += 4;
						break;
					case 2:
						border.StrokeShape = new RoundRectangle { CornerRadius = border.WidthRequest };
						break;
					case 3:
						border.Shadow.Radius = Math.Max(0, border.Shadow.Radius - 8);
						break;
					case 4:
						border.Clip = null;
						break;
					case 5:
						border.Shadow = null;
						break;
				}
			}

			if (view is Label label)
			{
				label.Text = labels[fn % labels.Length];
				if (fn == 5)
				{
					label.Shadow = null;
				}
			}
		}

		++fn;
		UpdateLabel();
	}

	void UpdateLabel()
	{
		TheLabel.Text = fn switch
		{
			1 => "Tap to resize the border",
			2 => "Tap to change the border shape",
			3 => "Tap to change the shadow radius",
			4 => "Tap to remove the clip",
			5 => "Tap to remove the shadow",
			_ => "Done"
		};
	}
}
