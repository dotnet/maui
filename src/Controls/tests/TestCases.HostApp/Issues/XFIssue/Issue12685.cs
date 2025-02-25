using Microsoft.Maui.Controls.Shapes;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 12685, "[iOs][Bug] TapGestureRecognizer in Path does not work on iOS", PlatformAffected.iOS)]

public partial class Issue12685 : TestContentPage
{
	const string StatusLabelId = "StatusLabelId";
	const string PathId = "PathId";

	const string ResetStatus = "Path touch event not fired, touch path above.";
	const string ClickedStatus = "Path was clicked, click reset button to start over.";

	protected override void Init()
	{
		var layout = new StackLayout();
		var statusLabel = new Label
		{
			AutomationId = StatusLabelId,
			Text = ResetStatus,
		};

		var lgb = new LinearGradientBrush();
		lgb.GradientStops.Add(new GradientStop(Colors.White, 0));
		lgb.GradientStops.Add(new GradientStop(Colors.Orange, 1));

		var pathGeometry = new PathGeometry();
		PathFigureCollectionConverter.ParseStringToPathFigureCollection(pathGeometry.Figures, "M0,0 V300 H300 V-300 Z");

		var path = new Path
		{
			AutomationId = PathId,
			Data = pathGeometry,
			Fill = lgb
		};

		var touch = new TapGestureRecognizer
		{
			Command = new Command(_ => statusLabel.Text = ClickedStatus),
		};
		path.GestureRecognizers.Add(touch);

		var resetButton = new Button
		{
			Text = "Reset",
			Command = new Command(_ => statusLabel.Text = ResetStatus),
		};

		layout.Children.Add(path);
		layout.Children.Add(statusLabel);
		layout.Children.Add(resetButton);

		Content = layout;
	}
}