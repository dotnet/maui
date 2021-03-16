using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Animation)]
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8004, "Add a ScaleXTo and ScaleYTo animation extension method", PlatformAffected.All)]
	public class Issue8004 : TestContentPage
	{
		BoxView _boxView;
		const string AnimateBoxViewButton = "AnimateBoxViewButton";
		const string BoxToScale = "BoxToScale";

		protected override void Init()
		{
			var label = new Label
			{
				Text = "Click the button below to animate the BoxView using individual ScaleXTo and ScaleYTo extension methods.",
				TextColor = Color.Black,
				AutomationId = "TestReady"
			};

			var button = new Button
			{
				AutomationId = AnimateBoxViewButton,
				Text = "Animate BoxView",
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				VerticalOptions = LayoutOptions.EndAndExpand
			};

			button.Clicked += AnimateButton_Clicked;

			_boxView = new BoxView
			{
				AutomationId = BoxToScale,
				BackgroundColor = Color.Blue,
				WidthRequest = 200,
				HeightRequest = 100,
				HorizontalOptions = LayoutOptions.Center
			};

			var grid = new Grid();

			Grid.SetRow(label, 0);
			Grid.SetRow(_boxView, 1);
			Grid.SetRow(button, 2);

			grid.Children.Add(label);
			grid.Children.Add(_boxView);
			grid.Children.Add(button);

			Content = grid;
		}

		void AnimateButton_Clicked(object sender, EventArgs e)
		{
			_boxView.ScaleYTo(2, 250, Easing.CubicInOut);
			_boxView.ScaleXTo(1.5, 400, Easing.BounceOut);
		}

#if UITEST
		[Test]
		public async Task AnimateScaleOfBoxView()
		{
			RunningApp.WaitForElement("TestReady");
			RunningApp.Screenshot("Small blue box");

			// Check the box and button elements.
			RunningApp.WaitForElement(q => q.Marked(BoxToScale));
			RunningApp.WaitForElement(q => q.Marked(AnimateBoxViewButton));

			// Tap the button.
			RunningApp.Tap(q => q.Marked(AnimateBoxViewButton));

			// Wait for animation to finish.
			await Task.Delay(500);
			   
			RunningApp.Screenshot("Bigger blue box");
		}
#endif
	}
}
