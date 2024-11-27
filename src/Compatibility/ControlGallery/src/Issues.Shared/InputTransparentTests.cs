using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.InputTransparent)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 8675309, "Test InputTransparent true/false on various controls")]
	public class InputTransparentTests : TestNavigationPage
	{
		const string TargetAutomationId = "inputtransparenttarget";

		static NavigationPage NavigationPage;

#if UITEST
		[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS] // Only menuItem=Frame fails on iOS
		[Test, TestCaseSource(nameof(TestCases))]
		public void VerifyInputTransparent(string menuItem)
		{
			if (menuItem == "BoxView" || menuItem == "Image" || menuItem == "Label")
			{
				Assert.Ignore("FailsOnMauiAndroid");
			}
			
			var results = RunningApp.WaitForElement(q => q.Marked(menuItem));

			if(results.Length > 1)
			{
				var rect = results.First(r => r.Class.Contains("Button")).Rect;

				RunningApp.TapCoordinates( rect.CenterX, rect.CenterY );
			}
			else
			{
				RunningApp.Tap(q => q.Marked(menuItem));
			}

			// Find the start label
			RunningApp.WaitForElement(q => q.Marked("Start"));

			// Find the control we're testing
			var result = RunningApp.WaitForElement(q => q.Marked(TargetAutomationId));

			// In theory we want to tap the center of the control. But Stepper lays out differently than the other controls,
			// (it doesn't center vertically within its layout), so we need to adjust for it until someone fixes it

#if __ANDROID__
			if (menuItem == "Stepper")
			{
				result = RunningApp.WaitForElement(q => q.Marked("−"));
			}
#endif

			var target = result.First().Rect;

			// Tap the control
			var y = target.CenterY;
			var x = target.CenterX;


			RunningApp.TapCoordinates(x, y);

			if(menuItem == nameof(DatePicker) || menuItem == nameof(TimePicker))
			{
				// These controls show a pop-up which we have to cancel/done out of before we can continue
#if __ANDROID__
				System.Threading.Tasks.Task.Delay(1000).Wait();
				RunningApp.Back();
#elif __IOS__
				var cancelButtonText = "Done";
				RunningApp.WaitForElement(q => q.Marked(cancelButtonText));
				RunningApp.Tap(q => q.Marked(cancelButtonText));
#endif
			}

			// Since InputTransparent is set to false, the start label should not have changed
			RunningApp.WaitForElement(q => q.Marked("Start"));

			// Switch to InputTransparent == true
			RunningApp.Tap(q => q.Marked("Toggle"));

			// Tap the control
			RunningApp.TapCoordinates(target.CenterX, target.CenterY);

			// Since InputTransparent is set to true, the start label should now show a single tap
			RunningApp.WaitForElement(q => q.Marked("Taps registered: 1"));

			NavigationPage = null;
		}
#endif

		static ContentPage CreateTestPage(View view)
		{
			var layout = new Grid();
			layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			layout.RowDefinitions.Add(new RowDefinition());

			var abs = new AbsoluteLayout();
			var box = new BoxView { Color = Colors.BlanchedAlmond };

			var label = new Label { BackgroundColor = Colors.Chocolate, Text = "Start", Margin = 5 };

			var taps = 0;

			abs.Children.Add(box, new Rect(0, 0, 1, 1), AbsoluteLayoutFlags.All);

			abs.Children.Add(label, new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);

			box.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					taps += 1;
					label.Text = $"Taps registered: {taps}";
				})
			});

			view.InputTransparent = false;
			abs.Children.Add(view, new Rect(.5, .5, .5, .5), AbsoluteLayoutFlags.All);

			var toggleButton = new Button { AutomationId = "Toggle", Text = $"Toggle InputTransparent (now {view.InputTransparent})" };
			toggleButton.Clicked += (sender, args) =>
			{
				view.InputTransparent = !view.InputTransparent;
				toggleButton.Text = $"Toggle InputTransparent (now {view.InputTransparent})";
			};

			layout.Children.Add(toggleButton);
			layout.Children.Add(abs);

			Grid.SetRow(abs, 1);

			return new ContentPage { Content = layout };
		}

		static Button MenuButton(string label, Func<View> view)
		{
			var button = new Button { Text = label };

			var testView = view();
			testView.AutomationId = TargetAutomationId;

			button.Clicked += (sender, args) => NavigationPage.PushAsync(CreateTestPage(testView));

			return button;
		}

		static IEnumerable<string> TestCases
		{
			get
			{
				return (BuildMenu().Content as Layout).SelectMany(
					element => (element as Layout).Select(view => (view as Button).Text));
			}
		}

		static ContentPage BuildMenu()
		{
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
			col1.Children.Add(MenuButton(nameof(Frame), () => new Frame { BackgroundColor = Colors.DarkGoldenrod }));
			col1.Children.Add(MenuButton(nameof(Entry), () => new Entry()));
			col1.Children.Add(MenuButton(nameof(Editor), () => new Editor()));
			col1.Children.Add(MenuButton(nameof(Button), () => new Button { Text = "Test" }));
			col1.Children.Add(MenuButton(nameof(Label), () => new Label
			{
				LineBreakMode = LineBreakMode.WordWrap,
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
			}));

			// We don't use 'SearchBar' here because on Android it sometimes finds the wrong control
			col1.Children.Add(MenuButton("TestSearchBar", () => new SearchBar()));

			col2.Children.Add(MenuButton(nameof(DatePicker), () => new DatePicker()));
			col2.Children.Add(MenuButton(nameof(TimePicker), () => new TimePicker()));
			col2.Children.Add(MenuButton(nameof(Slider), () => new Slider()));
			col2.Children.Add(MenuButton(nameof(Switch), () => new Switch()));
			col2.Children.Add(MenuButton(nameof(Stepper), () => new Stepper()));
			col2.Children.Add(MenuButton(nameof(BoxView), () => new BoxView { BackgroundColor = Colors.DarkMagenta, WidthRequest = 100, HeightRequest = 100 }));

			return new ContentPage { Content = layout };
		}

		protected override void Init()
		{
			NavigationPage = this;

			PushAsync(BuildMenu());
		}
	}
}
