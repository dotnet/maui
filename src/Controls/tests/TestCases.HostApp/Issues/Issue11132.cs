#if IOS || MACCATALYST
using System.Diagnostics;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
#endif

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 11132, "[Bug] [iOS] UpdateClip throws NullReferenceException when the Name of the Mask of the Layer is null", PlatformAffected.iOS)]
	public class Issue11132 : TestContentPage
	{
		const string InstructionsId = "instructions";

		public Issue11132()
		{

		}

		protected override void Init()
		{
			Title = "Issue 11132";

			var layout = new StackLayout();

			var instructions = new Label
			{
				AutomationId = InstructionsId,
				Padding = 12,
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "If the test works without exceptions (an orange rectangle is rendered), the test has passed."
			};

			var issue11132Control = new Issue11132Control
			{
				HeightRequest = 100
			};

			layout.Children.Add(instructions);
			layout.Children.Add(issue11132Control);

			Content = layout;
		}
	}


	public class Issue11132Control : View
	{

	}

#if IOS || MACCATALYST

	public class Issue11132ControlHandler : Microsoft.Maui.Handlers.ViewHandler<Issue11132Control, UIKit.UIView>
	{
		public Issue11132ControlHandler() : base(ViewHandler.ViewMapper, ViewHandler.ViewCommandMapper)
		{
		}

		protected override UIKit.UIView CreatePlatformView()
		{
			return new UIKit.UIView();
		}

		protected override void ConnectHandler(UIKit.UIView platformView)
		{
			base.ConnectHandler(platformView);

			var layer = platformView.Layer;

			if (layer != null)
			{
				layer.BorderWidth = 10;
				layer.BorderColor = Colors.Red.ToCGColor();
				layer.BackgroundColor = Colors.Orange.ToCGColor();

				var width = 100;
				var height = 25;

				var clipPath = new CoreGraphics.CGPath();
				clipPath.MoveToPoint(width, height);
				clipPath.AddLineToPoint(width * 2, height);
				clipPath.AddLineToPoint(width * 2, height * 2);
				clipPath.AddLineToPoint(width, height * 2);
				clipPath.CloseSubpath();

				var clipShapeLayer = new CAShapeLayer
				{
					Path = clipPath
				};
				layer.Mask = clipShapeLayer;
				layer.Mask.Name = null;

				Debug.WriteLine($"Issue11132ControlHandler Layer Name {layer.Mask.Name}");
			}
		}

		protected override void DisconnectHandler(UIKit.UIView platformView)
		{
			base.DisconnectHandler(platformView);
			// Cleanup if necessary
		}
	}




#endif

}