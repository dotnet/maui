namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25409, "UIButton CurrentImage can be set without crashing", PlatformAffected.iOS)]
public partial class Issue25409 : ContentPage
{
	public Issue25409()
	{
		InitializeComponent();
	}

#if IOS
	int count = 0;
#endif

	private void OnCounterClicked(object sender, EventArgs e)
	{
#if IOS
		((Action)(async () =>
		{
			if (count % 2 == 0)
			{
				var imageSource = new FileImageSource() { File = "shopping_cart.png" };

				if (button1.Handler?.MauiContext is not null && (button1.Handler.PlatformView is UIKit.UIButton platformButton))
				{
					var imageResult = await imageSource.GetPlatformImageAsync(button1.Handler.MauiContext);
					if (imageResult is not null)
					{
						platformButton.SetImage(imageResult.Value, UIKit.UIControlState.Normal);
					}
				}

				// since changing the UIButton.CurrentImage with SetImage does not trigger a layout pass,
				// we can change something else to force a layout pass
				button1.Text = string.Empty;
			}

			else
			{
				if (button1.Handler?.PlatformView is UIKit.UIButton platformButton)
				{
					platformButton.SetImage(null, UIKit.UIControlState.Normal);
				}

				// since changing the UIButton.CurrentImage with SetImage does not trigger a layout pass,
				// we can change something else to force a layout pass
				button1.Text = "Trigger Layout!";
			}

			count++;
		}))();
#endif
	}
}
