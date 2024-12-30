using Microsoft.Maui.Handlers;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Bugzilla, 60122, "LongClick on image not working", PlatformAffected.Android)]
public class Bugzilla60122 : TestContentPage
{
	const string ImageId = "60122Image";
	const string Success = "Success";

	protected override void Init()
	{
		var customImage = new _60122Image
		{
			AutomationId = ImageId,
			HeightRequest = 60,
			WidthRequest = 60,
			Source = "coffee.png"
		};

		var instructions = new Label
		{
			Text = $"Long press the image below; the label below it should change to read {Success}"
		};

		var result = new Label { Text = "Testing..." };

		customImage.LongPress += (sender, args) => { result.Text = Success; };

		Content = new StackLayout
		{
			Children = { instructions, customImage, result }
		};
	}
}

public class _60122Image : Image
{
	public event EventHandler LongPress;

	public void HandleLongPress(object sender, EventArgs e)
	{
		LongPress?.Invoke(this, new EventArgs());
	}
}

public class _60122ImageHandler : ImageHandler
{
	public _60122ImageHandler() : base(ImageHandler.Mapper)
	{
		Mapper.AppendToMapping("_60122ImageCustom", (handler, view) =>
		{
			if (view is _60122Image customImage)
			{
#if IOS || MACCATALYST
				if (handler.PlatformView is UIKit.UIImageView uiImageView)
				{
					var gesture = new UIKit.UILongPressGestureRecognizer(OnLongPress);
					uiImageView.AddGestureRecognizer(gesture);
					uiImageView.UserInteractionEnabled = true;
				}
#elif ANDROID
				if (handler.PlatformView is Android.Widget.ImageView androidImageView)
				{
					androidImageView.LongClickable = true;
					androidImageView.LongClick += OnLongPress;
				}
#elif WINDOWS
				if (handler.PlatformView is Microsoft.UI.Xaml.Controls.Image winImage)
				{
					var _gestureRecognizer = new global::Windows.UI.Input.GestureRecognizer();
					_gestureRecognizer.GestureSettings = global::Windows.UI.Input.GestureSettings.HoldWithMouse;
					winImage.Holding += OnHolding;
				}
#endif
			}
		});
	}

#if WINDOWS
	void OnHolding(object sender, Microsoft.UI.Xaml.Input.HoldingRoutedEventArgs holdingRoutedEventArgs)
	{
		if (holdingRoutedEventArgs.HoldingState == Microsoft.UI.Input.HoldingState.Completed && VirtualView is _60122Image customImage)
		{
			customImage?.HandleLongPress(customImage, new EventArgs());
		}
	}
#elif IOS || MACCATALYST
	void OnLongPress()
	{
		if (VirtualView is _60122Image customImage)
		{
			customImage.HandleLongPress(customImage, EventArgs.Empty);
		}
	}
#elif ANDROID
	void OnLongPress(object sender, EventArgs e)
	{
		if (VirtualView is _60122Image customImage)
		{
			customImage.HandleLongPress(customImage, EventArgs.Empty);
		}
	}
#endif
}