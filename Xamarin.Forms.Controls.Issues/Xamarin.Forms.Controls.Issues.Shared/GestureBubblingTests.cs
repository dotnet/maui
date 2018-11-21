using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest.Queries;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	// This is similar to the test for 35477, but tests all of the basic controls to make sure that they all exhibit
	// the same behavior across all the platforms. The question is whether tapping a control inside of a frame
	// will trigger the frame's tap gesture; for most controls it will not (the control itself absorbs the tap),
	// but for non-interactive controls (box, frame, image, label) the gesture bubbles up to the container.

#if UITEST
	[Category(UITestCategories.Gestures)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 00100100, "Verify that the tap gesture bubbling behavior is consistent across the platforms", PlatformAffected.All)]
	public class GestureBubblingTests : TestNavigationPage
	{
		const string TargetAutomationId = "controlinsideofframe";
		const string NoTaps = "No taps yet";
		const string Tapped = "Frame was tapped";
		ContentPage _menu;

#if UITEST
		[Test, TestCaseSource(nameof(TestCases))]
		public void VerifyTapBubbling(string menuItem, bool frameShouldRegisterTap)
		{
			var results = RunningApp.WaitForElement(q => q.Marked(menuItem));

			if (results.Length > 1)
			{
				var rect = results.First(r => r.Class.Contains("Button")).Rect;

				RunningApp.TapCoordinates(rect.CenterX, rect.CenterY);
			}
			else
			{
				RunningApp.Tap(q => q.Marked(menuItem));
			}

			// Find the start label
			RunningApp.WaitForElement(q => q.Marked(NoTaps));

			// Find the control we're testing
			var result = RunningApp.WaitForElement(q => q.Marked(TargetAutomationId));
			var target = result.First().Rect;

			// Tap the control
			var y = target.CenterY;
			var x = target.CenterX;

			// In theory we want to tap the center of the control. But Stepper lays out differently than the other controls,
			// so we need to adjust for it until someone fixes it
			if (menuItem == "Stepper")
			{
				y = target.Y + 5;
				x = target.X + 5;
			}

			RunningApp.TapCoordinates(x, y);

			if (menuItem == nameof(DatePicker) || menuItem == nameof(TimePicker))
			{
				// These controls show a pop-up which we have to cancel/done out of before we can continue
#if __ANDROID__
				var cancelButtonText = "Cancel";
				RunningApp.Back();
#elif __IOS__
				var cancelButtonText = "Done";
				RunningApp.WaitForElement(q => q.Marked(cancelButtonText));
				RunningApp.Tap(q => q.Marked(cancelButtonText));
#else
				var cancelButtonText = "DismissButton";
#endif

			}

			if (frameShouldRegisterTap)
			{
				RunningApp.WaitForElement(q => q.Marked(Tapped));
			}
			else
			{
				RunningApp.WaitForElement(q => q.Marked(NoTaps));
			}
		}
#endif

		ContentPage CreateTestPage(View view)
		{
			var instructions = new Label();

			if (_controlsWhichShouldAllowTheTapToBubbleUp.Contains(view.GetType().Name))
			{
				instructions.Text =
					$"Tap the frame below. The label with the text '{NoTaps}' should change its text to '{Tapped}'.";
			}
			else
			{
				instructions.Text =
					$"Tap the frame below. The label with the text '{NoTaps}' should not change.";
			}

			var label = new Label { Text = NoTaps };

			var frame = new Frame { Content = new StackLayout { Children = { view } } };

			var rec = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
			rec.Tapped += (s, e) => { label.Text = Tapped; };
			frame.GestureRecognizers.Add(rec);

			var layout = new StackLayout();

			layout.Children.Add(instructions);
			layout.Children.Add(label);
			layout.Children.Add(frame);

			return new ContentPage { Content = layout };
		}

		Button MenuButton(string label, Func<View> view)
		{
			var button = new Button { Text = label };

			var testView = view();
			testView.AutomationId = TargetAutomationId;

			button.Clicked += (sender, args) => PushAsync(CreateTestPage(testView));

			return button;
		}

		// These controls should allow the tap gesture to bubble up to their container; everything else should absorb the gesture
		readonly List<string> _controlsWhichShouldAllowTheTapToBubbleUp = new List<string>
		{
			nameof(Image),
			nameof(Label),
			nameof(BoxView),
			nameof(Frame)
		};

		IEnumerable<object[]> TestCases
		{
			get
			{
				var layout = BuildMenu().Content as Layout;
				var result =
					from Layout element in layout.InternalChildren
					from Button button in element.InternalChildren
					let text = button.Text
					// UwpIgnore
#if __WINDOWS__
					where text != "Stepper" && text != "Entry"
#endif
					select new object[]
					{
						text,
						_controlsWhichShouldAllowTheTapToBubbleUp.Contains(text)
					};

				return result;
			}
		}

		ContentPage BuildMenu()
		{
			if (_menu != null)
			{
				return _menu;
			}

			var layout = new Grid
			{
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
				ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(), new ColumnDefinition() }
			};

			var col1 = new StackLayout();
			layout.Children.Add(col1);
			Grid.SetColumn(col1, 0);

			var col2 = new StackLayout();
			layout.Children.Add(col2);
			Grid.SetColumn(col2, 1);

			col1.Children.Add(MenuButton(nameof(Image), () => new Image { Source = ImageSource.FromFile("oasis.jpg") }));
			col1.Children.Add(MenuButton(nameof(Frame), () => new Frame { BackgroundColor = Color.DarkGoldenrod }));
			col1.Children.Add(MenuButton(nameof(Entry), () => new Entry()));
			col1.Children.Add(MenuButton(nameof(Editor), () => new Editor()));
			col1.Children.Add(MenuButton(nameof(Button), () => new Button { Text = "Test" }));
			col1.Children.Add(MenuButton(nameof(Label), () => new Label
			{
				LineBreakMode = LineBreakMode.WordWrap,
				Text = "Lorem ipsum dolor sit amet"
			}));

			// We don't use 'SearchBar' here because on Android it sometimes finds the wrong control
			col1.Children.Add(MenuButton("TestSearchBar", () => new SearchBar()));

			col2.Children.Add(MenuButton(nameof(DatePicker), () => new DatePicker()));
			col2.Children.Add(MenuButton(nameof(TimePicker), () => new TimePicker()));

			var slider = new Slider();
			slider.On<iOS>().SetUpdateOnTap(true);
			col2.Children.Add(MenuButton(nameof(Slider), () => slider));

			col2.Children.Add(MenuButton(nameof(Switch), () => new Switch()));
			col2.Children.Add(MenuButton(nameof(Stepper), () => new Stepper()));
			col2.Children.Add(MenuButton(nameof(BoxView), () => new BoxView { BackgroundColor = Color.DarkMagenta, WidthRequest = 100, HeightRequest = 100 }));

			return new ContentPage { Content = layout };
		}

		protected override void Init()
		{
			PushAsync(BuildMenu());
		}
	}
}