using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41029, "Slider default hitbox is larger than the control")]
	public class Bugzilla41029 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var stackLayout = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Color.Beige
			};

			var label = new Label
			{
				Margin = new Thickness(10, 0),
				LineBreakMode = LineBreakMode.WordWrap,
				Text = "Tap several times somewhere on the screen below Button 3 and verify that the slider's circle does not change size or animate. If so, the test has passed."
			};
			var button = new Button
			{
				Text = "Button 1"
			};
			var slider = new Slider
			{
				Minimum = 0,
				Maximum = 100,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
			var button2 = new Button
			{
				Text = "Button 2"
			};
			var button3 = new Button
			{
				Text = "Button 3"
			};

			stackLayout.Children.Add(label);
			stackLayout.Children.Add(button);
			stackLayout.Children.Add(slider);
			stackLayout.Children.Add(button2);
			stackLayout.Children.Add(button3);

			Content = stackLayout;
		}
	}
}