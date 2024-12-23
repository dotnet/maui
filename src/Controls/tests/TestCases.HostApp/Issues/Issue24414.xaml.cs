using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24414, "Shadows not rendering as expected on Android and iOS", PlatformAffected.Android | PlatformAffected.iOS)]

public partial class Issue24414 : ContentPage
{
	int fn = 0;
	string[] labels = ["LLOHE WODRL", "OLLEH ORLWD", "LOHEL LODRW", "HELLO WORLD"];

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
					case 0:
						border.WidthRequest += 4;
						border.HeightRequest += 4;
						break;
					case 1:
						border.StrokeShape = new RoundRectangle { CornerRadius = border.WidthRequest };
						break;
					case 2:
						border.Shadow.Radius -= 8;
						break;
				}
			}

			if (view is Label label)
			{
				label.Text = labels[fn % labels.Length];
			}
		}

		++fn;
		UpdateLabel();
	}

	void UpdateLabel()
	{
		TheLabel.Text = fn switch
		{
			0 => "Tap to resize the border",
			1 => "Tap to change the border shape",
			2 => "Tap to change the shadow radius",
			_ => "Done"
		};
	}
}
