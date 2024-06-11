using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
#endif


namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 773, "Horizontal ScrollView locks after rotation", PlatformAffected.iOS)]
	public class Issue773 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new CannotScrollRepro());
		}

#if UITEST
		[Compatibility.UITests.MovedToAppium]
		[Test]
		[Issue (IssueTracker.Github, 773, "Horizontal ScrollView locks after rotation - relayout correctly after rotation", PlatformAffected.iOS)]
		[UiTest (typeof(ScrollView))]
		[Compatibility.UITests.FailsOnMauiAndroid]
		public void Issue773TestsRotationRelayoutIssue ()
		{
			RunningApp.SetOrientationLandscape ();

			var buttonLabels = new [] { 
				"Button 1",
				"Button 2",
				"Button 3",
			};

			foreach (string buttonLabel in buttonLabels)
				RunningApp.WaitForElement (q => q.Button (buttonLabel));

			RunningApp.Screenshot ("StackLayout in Modal respects rotation");

			RunningApp.SetOrientationPortrait ();
		}
#endif
	}

	public class CannotScrollRepro : ContentPage
	{
		public CannotScrollRepro()
		{
			Title = "Nav Bar";

			var layout = new StackLayout
			{
				Padding = new Thickness(20),
				BackgroundColor = Colors.Gray
			};

			var button1 = new Button { Text = "Button 1" };
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
				Children = {
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
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand
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
