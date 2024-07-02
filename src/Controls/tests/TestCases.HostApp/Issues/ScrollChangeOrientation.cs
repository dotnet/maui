using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	// Issue773TestsRotationRelayoutIssue (src\Compatibility\ControlGallery\src\Issues.Shared\Issue773.cs)
	[Issue(IssueTracker.None, 773, "Horizontal ScrollView locks after rotation", PlatformAffected.iOS)]
	public class ScrollChangeOrientation : ContentPage
	{
		public ScrollChangeOrientation()
		{
			Title = "Can scroll changing the Orientation";

			var layout = new StackLayout
			{
				Padding = new Thickness(20),
				BackgroundColor = Colors.Gray
			};

			var button1 = new Button { AutomationId = "Button1", Text = "Button 1" };
			var button2 = new Button { AutomationId = "Button2", Text = "Button 2", IsEnabled = false };
			var button3 = new Button { AutomationId = "Button3", Text = "Button 3", IsEnabled = false };
			var button4 = new Button { AutomationId = "Button4", Text = "Button 4", IsEnabled = false };
			var button5 = new Button { AutomationId = "Button5", Text = "Button 5", IsEnabled = false };
			var button6 = new Button { AutomationId = "Button6", Text = "Button 6", IsEnabled = false };
			var button7 = new Button { AutomationId = "Button7", Text = "Button 7", IsEnabled = false };
			var button8 = new Button { AutomationId = "Button8", Text = "Button 8" };

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
