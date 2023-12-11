using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.Issues
{
	// Issue773TestsRotationRelayoutIssue (src\Compatibility\ControlGallery\src\Issues.Shared\Issue773.cs)
	[Issue(IssueTracker.None, 0, "Horizontal ScrollView locks after rotation", PlatformAffected.iOS)]
	public class ScrollViewDeviceOrientation : ContentPage
	{
		public ScrollViewDeviceOrientation()
		{
			Title = "ScrollView DeviceOrientation";

			var layout = new StackLayout
			{
				Padding = new Thickness(20),
				BackgroundColor = Colors.Gray
			};

			var button1 = new Button { AutomationId = "WaitForStubControl", Text = "Button 1" };
			var button2 = new Button { Text = "Button 2", IsEnabled = false };
			var button3 = new Button { Text = "Button 3", IsEnabled = false };
			var button4 = new Button { Text = "Button 4", IsEnabled = false };
			var button5 = new Button { Text = "Button 5", IsEnabled = false };
			var button6 = new Button { Text = "Button 6", IsEnabled = false };
			var button7 = new Button { Text = "Button 7", IsEnabled = false };
			var button8 = new Button { Text = "Button 8" };

			var label = new Label { Text = "Not Clicked" };

			var buttonStack = new StackLayout
			{
				Padding = new Thickness(30, 0),
				Orientation = StackOrientation.Horizontal,
				Spacing = 30,
				Children =
				{
					button1,
					button2,
					button3,
					button4,
					button5,
					button6,
					button7,
					button8,
				}
			};

			button1.Clicked += (sender, args) => Navigation.PopModalAsync();

			int count = 0;
			button8.Clicked += (sender, e) =>
			{
				if (count == 0)
				{
					label.Text = "I was clicked once!";
					count++;
				}
				else if (count == 1)
				{
					label.Text = "I was clicked again!";
					count++;
				}
				else if (count == 2)
				{
					label.Text = "I was clicked again again!";
				}
			};

			layout.Children.Add(new BoxView
			{
				BackgroundColor = Colors.Red,
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill
			});

			layout.Children.Add(label);

			layout.Children.Add(new ScrollView
			{
				BackgroundColor = Colors.Aqua,
				Orientation = ScrollOrientation.Horizontal,
				HeightRequest = DeviceInfo.Platform == DevicePlatform.WinUI ? 80 : 44,
				Content = buttonStack
			});

			Content = layout;
		}
	}
}