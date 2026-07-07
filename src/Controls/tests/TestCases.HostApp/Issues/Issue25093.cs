namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25093, "[iOS] TintColor on UIButton image no longer working when button made visible", PlatformAffected.iOS)]
public class Issue25093 : ContentPage
{
	public Issue25093()
	{
		var tintedButton = new Button
		{
			AutomationId = "TintedButton",
			ImageSource = "red.png",
			BackgroundColor = Colors.Black,
			WidthRequest = 200,
			HeightRequest = 200
		};

		var statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Waiting"
		};

		var applyTintButton = new Button
		{
			Text = "Apply Tint and Resize",
			AutomationId = "ApplyTintButton"
		};

		applyTintButton.Clicked += (s, e) =>
		{
#if IOS || MACCATALYST
			if (tintedButton.Handler?.PlatformView is UIKit.UIButton platformButton)
			{
				// Simulate IconTintColorBehavior: set AlwaysTemplate + tint color
				var image = platformButton.ImageView?.Image?.ImageWithRenderingMode(UIKit.UIImageRenderingMode.AlwaysTemplate);
				if (image is not null)
				{
					platformButton.SetImage(image, UIKit.UIControlState.Normal);
					platformButton.TintColor = UIKit.UIColor.Green;
					platformButton.ImageView.TintColor = UIKit.UIColor.Green;
				}

				// Verify rendering mode is preserved after resize triggers ResizeImageIfNecessary
				void OnSizeChanged(object sender, EventArgs args)
				{
					tintedButton.SizeChanged -= OnSizeChanged;
					var currentMode = platformButton.ImageView?.Image?.RenderingMode;
					statusLabel.Text = currentMode == UIKit.UIImageRenderingMode.AlwaysTemplate
						? "PASS"
						: $"FAIL: RenderingMode is {currentMode}";
				}

				tintedButton.SizeChanged += OnSizeChanged;

				// Shrink to trigger ResizeImageIfNecessary during the next measure pass
				tintedButton.WidthRequest = 100;
				tintedButton.HeightRequest = 100;
			}
			else
			{
				statusLabel.Text = "PASS";
			}
#else
			statusLabel.Text = "PASS";
#endif
		};

		Content = new VerticalStackLayout
		{
			Children = { applyTintButton, tintedButton, statusLabel }
		};
	}
}
