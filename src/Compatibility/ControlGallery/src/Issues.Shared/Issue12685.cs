using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12685, "[iOs][Bug] TapGestureRecognizer in Path does not work on iOS", PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.Shape)]
#endif
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
			lgb.GradientStops.Add(new GradientStop(Color.White, 0));
			lgb.GradientStops.Add(new GradientStop(Color.Orange, 1));

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

#if UITEST
		[Test]
		public void ShapesPathReceiveGestureRecognizers()
		{
			var testLabel = RunningApp.WaitForFirstElement(StatusLabelId);
			Assert.AreEqual(ResetStatus, testLabel.ReadText());
			var testPath = RunningApp.WaitForFirstElement(PathId);
			var pathRect = testPath.Rect;
			RunningApp.TapCoordinates(pathRect.X + 1, pathRect.Y + 1);
			Assert.AreEqual(ClickedStatus, RunningApp.WaitForFirstElement(StatusLabelId).ReadText());
		}
#endif
	}
}