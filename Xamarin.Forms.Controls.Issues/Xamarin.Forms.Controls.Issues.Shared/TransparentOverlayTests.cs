using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.InputTransparent)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 618, "Transparent Overlays", PlatformAffected.All)]
	public class TransparentOverlayTests : TestNavigationPage
	{
		readonly Color _transparentColor = Color.Transparent;
		readonly Color _nontransparentColor = Color.Blue;

		double _transparentOpacity = 0;
		double _nonTransparentOpacity = 0.2;

        const string Running = "Running...";
		const string Success = "Success";
        const string Failure = "Failure";
		const string DefaultButtonText = "Button";
		const string Overlay = "overlay";
		const string AddOverlay = "Add Overlay";

		protected override void Init()
		{
			PushAsync(Menu());
		}

		ContentPage Menu()
		{
			var layout = new StackLayout();

			layout.Children.Add(new Label {Text = "Select a test below"});

			foreach (var test in GenerateTests)
			{
				layout.Children.Add(MenuButton(test));
			}

			return new ContentPage { Content = layout };
		}

		Button MenuButton(TestPoint test)
		{
			var button = new Button { Text = test.ToString(), AutomationId = test.AutomationId };

			button.Clicked += (sender, args) => PushAsync(CreateTestPage(test));

			return button;
		}

		[Preserve(AllMembers = true)]
		public struct TestPoint
		{
			public TestPoint(int i) : this()
			{
				AutomationId = $"transparenttest{i}";

				Opacity = (i & (1 << 0)) == 0;
				InputTransparent = (i & (1 << 1)) == 0;
				BackgroundColor = (i & (1 << 2)) == 0;

				// Layouts should be input transparent _only_ if they were explicitly told to be
				ShouldBeTransparent = InputTransparent;
			}

			internal string AutomationId { get; set; }
			internal bool ShouldBeTransparent { get; set; }

			internal bool Opacity { get; set; }
			internal bool InputTransparent { get; set; }
			internal bool BackgroundColor { get; set; }
			
			public override string ToString()
			{
				return $"O{(Opacity ? "1" : "0")}, B{(BackgroundColor ? "1" : "0")}, I{(InputTransparent ? "1" : "0")}";
			}
		}

		static IEnumerable<TestPoint> GenerateTests
		{
			get { return Enumerable.Range(0, 8).Select(i => new TestPoint(i)); }
		}

		ContentPage CreateTestPage(TestPoint test)
		{
			Color backgroundColor = test.BackgroundColor? _transparentColor : _nontransparentColor;
			double opacity = test.Opacity ? _transparentOpacity : _nonTransparentOpacity;
			bool inputTransparent = test.InputTransparent;

            var grid = new Grid
            {
                AutomationId = "testgrid",
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};

			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

			var instructions = new Label
			{
				HorizontalOptions = LayoutOptions.Fill,
				HorizontalTextAlignment = TextAlignment.Center,
				Text = $"Tap the 'Add Overlay' button, then tap the button labeled 'Button'."
				       + (test.ShouldBeTransparent
					       ? $" If the label below's text changes to {Success} the test has passed."
					       : " If the label below's text remains unchanged, the test has passed.")
			};

			grid.Children.Add(instructions);

            var results = new Label 
            { 
                HorizontalOptions = LayoutOptions.Fill,
                HorizontalTextAlignment = TextAlignment.Center, 
                Text = Running 
            };

            grid.Children.Add(results);
            Grid.SetRow(results, 1);

			var button = new Button
			{
				Text = DefaultButtonText,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			button.Clicked += (sender, args) =>
			{
				results.Text = test.ShouldBeTransparent ? Success : Failure;
			};

			var layout = new StackLayout
			{
                AutomationId = Overlay,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				BackgroundColor = backgroundColor,
				InputTransparent = inputTransparent,
				Opacity = opacity
			};

			// Bump up the elevation to cover FastRenderer buttons
			layout.On<Android>().SetElevation(10f);

			grid.Children.Add(button);
			Grid.SetRow(button, 3);

			var addOverlayButton = new Button() { Text = AddOverlay };
			addOverlayButton.Clicked += (sender, args) =>
			{
				grid.Children.Add(layout);
				Grid.SetRow(layout, 3);
			};

			grid.Children.Add(addOverlayButton);
			Grid.SetRow(addOverlayButton, 2);

			return new ContentPage { Content = grid, Title = test.ToString()};
		}

#if UITEST
        [Test, TestCaseSource(nameof(GenerateTests))]
        public void VerifyInputTransparent(TestPoint test)
        {
            RunningApp.WaitForElement(q => q.Marked(test.AutomationId));
            RunningApp.Tap(test.AutomationId);

			// Determine the location of the button; we have to do this before adding the overlay because
			// otherwise the iOS UI tests won't be able to find it consistently
			var button = RunningApp.WaitForElement(DefaultButtonText);
			var coords = new Tuple<float, float>(button[0].Rect.CenterX, button[0].Rect.CenterY);

			// Add the overlay 
			RunningApp.WaitForElement(AddOverlay);
			RunningApp.Tap(AddOverlay);

			// Now tap the screen at the Button's location
			// We can't do RunningApp.Tap(DefaultButtonText) because the UI tests on iOS can't see it
			RunningApp.TapCoordinates(coords.Item1, coords.Item2);

			// Check the results
            RunningApp.WaitForElement(test.ShouldBeTransparent ? Success : Running); 
		}
#endif

	}
}