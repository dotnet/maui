using System;
using System.Xml.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;

#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 34007, "Z order drawing of children views are different on Android, iOS, Win", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Bugzilla34007 : TestContentPage
	{
		protected override void Init ()
		{
			var grid = new Grid ();

			var button0 = new Button {
				Text = "Button 0",
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};

			var button1 = new Button {
				Text = "Button 1",
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};

			var lastButtonTappedLabel = new Label ();

			Action reorder = () => {
				// Get the last item in the grid
				var item = grid.Children[1];

				// Remove it
				grid.Children.RemoveAt(1);

				// And put it back as the 0th item
				grid.Children.Insert(0, item);
			};

			button0.Clicked += (sender, args) => {
				lastButtonTappedLabel.Text = "Button 0 was tapped last";
			};

			button1.Clicked += (sender, args) => {
				lastButtonTappedLabel.Text = "Button 1 was tapped last";

				reorder ();
			};

			grid.Children.Add(button0, 0, 0);
			grid.Children.Add(button1, 0, 0);

			Content = new StackLayout {
				Children = { grid, lastButtonTappedLabel }
			};
		}

#if UITEST
		[Test]
		[UiTest (typeof (Grid))]
		public void Issue34007TestFirstElementHasLowestZOrder ()
		{
			var buttonLocations = RunningApp.WaitForElement (q => q.Marked ("Button 0"));

			var x = buttonLocations [0].Rect.CenterX;
			var y = buttonLocations [0].Rect.CenterY;

			// Button 1 was the last item added to the grid; it should be tappable
			RunningApp.Tap (q => q.Button ("Button 1"));

			// The label should indicate that Button 1 was the last button tapped
			RunningApp.WaitForElement(q => q.Marked("Button 1 was tapped last"));

			RunningApp.Screenshot("Buttons Reordered");

			// Tapping Button1 1 reordered the buttons in the grid; Button 0 should
			// now be on top. Tapping at the Button 1 location should actually tap
			// Button 0, and the label should indicate that
			RunningApp.TapCoordinates(x, y);
			RunningApp.WaitForElement(q => q.Marked("Button 0 was tapped last"));

			RunningApp.Screenshot("Button 0 Tapped");
		}
#endif
	}
}