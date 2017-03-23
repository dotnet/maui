using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.IsEnabled)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44096, "Grid, StackLayout, and ContentView still participate in hit testing on Android after IsEnabled is set to false", PlatformAffected.Android)]
	public class Bugzilla44096 : TestContentPage
	{
		bool _flag;

		protected override void Init()
		{
			var result = new Label
			{
				Text = "Original"
			};

			var grid = new Grid
			{
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = "grid"
			};
			AddTapGesture(result, grid);

			var contentView = new ContentView
			{
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = "contentView"
			};
			AddTapGesture(result, contentView);

			var stackLayout = new StackLayout
			{
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = "stackLayout"
			};
			AddTapGesture(result, stackLayout);

			var color = new Button
			{
				Text = "Toggle colors",
				Command = new Command(() =>
				{
					if (!_flag)
					{
						grid.BackgroundColor = Color.Red;
						contentView.BackgroundColor = Color.Blue;
						stackLayout.BackgroundColor = Color.Yellow;
					}
					else
					{
						grid.BackgroundColor = Color.Default;
						contentView.BackgroundColor = Color.Default;
						stackLayout.BackgroundColor = Color.Default;
					}

					_flag = !_flag;
				}),
				AutomationId = "color"
			};

			var disabled = new Button
			{
				Text = "Disabled",
				Command = new Command(() =>
				{
					grid.IsEnabled = false;
					contentView.IsEnabled = false;
					stackLayout.IsEnabled = false;

					result.Text = "Original";
				}),
				AutomationId = "disabled"
			};

			var parent = new StackLayout
			{
				Spacing = 10,
				Orientation = StackOrientation.Vertical,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					color,
					disabled,
					result,
					grid,
					contentView,
					stackLayout
				}
			};

			Content = parent;
		}

		void AddTapGesture(Label result, View view)
		{
			var tapGestureRecognizer = new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					result.Text = "Child";
				})
			};
			view.GestureRecognizers.Add(tapGestureRecognizer);
		}

#if UITEST
		[Test]
		public void Test()
		{
			RunningApp.WaitForElement(q => q.Marked("grid"));
			RunningApp.Tap(q => q.Marked("grid"));
			RunningApp.WaitForElement(q => q.Marked("Child"));

			RunningApp.WaitForElement(q => q.Marked("contentView"));
			RunningApp.Tap(q => q.Marked("contentView"));
			RunningApp.WaitForElement(q => q.Marked("Child"));

			RunningApp.WaitForElement(q => q.Marked("stackLayout"));
			RunningApp.Tap(q => q.Marked("stackLayout"));
			RunningApp.WaitForElement(q => q.Marked("Child"));

			RunningApp.WaitForElement(q => q.Marked("color"));
			RunningApp.Tap(q => q.Marked("color"));

			RunningApp.WaitForElement(q => q.Marked("grid"));
			RunningApp.Tap(q => q.Marked("grid"));
			RunningApp.WaitForElement(q => q.Marked("Child"));

			RunningApp.WaitForElement(q => q.Marked("contentView"));
			RunningApp.Tap(q => q.Marked("contentView"));
			RunningApp.WaitForElement(q => q.Marked("Child"));

			RunningApp.WaitForElement(q => q.Marked("stackLayout"));
			RunningApp.Tap(q => q.Marked("stackLayout"));
			RunningApp.WaitForElement(q => q.Marked("Child"));

			RunningApp.WaitForElement(q => q.Marked("disabled"));
			RunningApp.Tap(q => q.Marked("disabled"));

			RunningApp.WaitForElement(q => q.Marked("grid"));
			RunningApp.Tap(q => q.Marked("grid"));
			RunningApp.WaitForElement(q => q.Marked("Original"));

			RunningApp.WaitForElement(q => q.Marked("contentView"));
			RunningApp.Tap(q => q.Marked("contentView"));
			RunningApp.WaitForElement(q => q.Marked("Original"));

			RunningApp.WaitForElement(q => q.Marked("stackLayout"));
			RunningApp.Tap(q => q.Marked("stackLayout"));
			RunningApp.WaitForElement(q => q.Marked("Original"));

			RunningApp.WaitForElement(q => q.Marked("color"));
			RunningApp.Tap(q => q.Marked("color"));

			RunningApp.WaitForElement(q => q.Marked("grid"));
			RunningApp.Tap(q => q.Marked("grid"));
			RunningApp.WaitForElement(q => q.Marked("Original"));

			RunningApp.WaitForElement(q => q.Marked("contentView"));
			RunningApp.Tap(q => q.Marked("contentView"));
			RunningApp.WaitForElement(q => q.Marked("Original"));

			RunningApp.WaitForElement(q => q.Marked("stackLayout"));
			RunningApp.Tap(q => q.Marked("stackLayout"));
			RunningApp.WaitForElement(q => q.Marked("Original"));
		}
#endif
	}
}