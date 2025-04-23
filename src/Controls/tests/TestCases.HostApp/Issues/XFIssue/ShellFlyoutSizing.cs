namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Shell Flyout Width and Height",
	PlatformAffected.All)]
public class ShellFlyoutSizing : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Locked; //After tapping on items, the flyout closes on Windows. To ensure uniform behavior across platforms, set the FlyoutBehavior to Locked.
		AddContentPage(new ContentPage()
		{
			Title = "Main Page",
			Content = new StackLayout()
			{
				new Label()
				{
					Text = "Open the Flyout and click the button. The height and width should change. Click it again and it should go back to default",
					AutomationId ="PageLoaded"
				}
			}
		});

		FlyoutBackground = new LinearGradientBrush
		{
			StartPoint = new Point(0, 0),
			EndPoint = new Point(1, 0),
			GradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Colors.Blue, Offset = 0.1f },
				new GradientStop { Color = Colors.BlueViolet, Offset = 1.0f },
			}
		};

		FlyoutHeader = new Label()
		{
			BackgroundColor = Colors.LightBlue,
			Text = "Header",
			AutomationId = "FlyoutHeader"
		};


		FlyoutFooter = new Label()
		{
			BackgroundColor = Colors.LightBlue,
			Text = "Footer",
			AutomationId = "FlyoutFooter"
		};


		var scale = 10d;

		if (OperatingSystem.IsAndroid())
		{
			//TODO: use right essentials api
			//scale = scale / DeviceDisplay.MainDisplayInfo.Density;
		}

		var increaseMenuItem = new MenuItem()
		{
			Text = "Increase Height and Width",
			Command = new Command(() =>
			{
				FlyoutWidth += scale;
				FlyoutHeight += scale;
			}),
			AutomationId = "IncreaseFlyoutSizes"
		};

		var descreaseMenuItem = new MenuItem()
		{
			Text = "Decrease Height and Width",
			Command = new Command(() =>
			{
				FlyoutWidth -= scale;
				FlyoutHeight -= scale;
			}),
			AutomationId = "DecreaseFlyoutSizes"
		};


		Items.Add(new MenuItem()
		{
			Text = "Change Height and Width",
			Command = new Command(() =>
			{
				FlyoutWidth = 350;
				FlyoutHeight = 350;
				Items.Add(increaseMenuItem);
				Items.Add(descreaseMenuItem);
			}),
			AutomationId = "ChangeFlyoutSizes"
		});

		Items.Add(new MenuItem()
		{
			Text = "Reset Height and Width",
			Command = new Command(() =>
			{
				FlyoutWidth = -1;
				FlyoutHeight = -1;
				Items.Remove(increaseMenuItem);
				Items.Remove(descreaseMenuItem);
			}),
			AutomationId = "ResetFlyoutSizes"
		});
	}
}
