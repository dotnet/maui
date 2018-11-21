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
	[Category(UITestCategories.InputTransparent)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44176, "InputTransparent fails if BackgroundColor not explicitly set on Android", PlatformAffected.Android)]
	public class Bugzilla44176 : TestContentPage
	{
		bool _flag;

		protected override void Init()
		{
			var result = new Label();

			var grid = new Grid
			{
				InputTransparent = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = "grid"
			};
			AddTapGesture(result, grid);

			var contentView = new ContentView
			{
				InputTransparent = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = "contentView"
			};
			AddTapGesture(result, contentView);

			var stackLayout = new StackLayout
			{
				InputTransparent = true,
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

			var nonTransparent = new Button
			{
				Text = "Non-transparent",
				Command = new Command(() =>
				{
					grid.InputTransparent = false;
					contentView.InputTransparent = false;
					stackLayout.InputTransparent = false;
				}),
				AutomationId = "nontransparent"
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
					nonTransparent,
					result,
					grid,
					contentView,
					stackLayout
				}
			};
			AddTapGesture(result, parent, true);

			Content = parent;
		}

		void AddTapGesture(Label result, View view, bool isParent = false)
		{
			var tapGestureRecognizer = new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					result.Text = !isParent ? "Child" : "Parent";
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
			RunningApp.WaitForElement(q => q.Marked("Parent"));

			RunningApp.WaitForElement(q => q.Marked("contentView"));
			RunningApp.Tap(q => q.Marked("contentView"));
			RunningApp.WaitForElement(q => q.Marked("Parent"));

			RunningApp.WaitForElement(q => q.Marked("stackLayout"));
			RunningApp.Tap(q => q.Marked("stackLayout"));
			RunningApp.WaitForElement(q => q.Marked("Parent"));

			RunningApp.WaitForElement(q => q.Marked("color"));
			RunningApp.Tap(q => q.Marked("color"));

			RunningApp.WaitForElement(q => q.Marked("grid"));
			RunningApp.Tap(q => q.Marked("grid"));
			RunningApp.WaitForElement(q => q.Marked("Parent"));

			RunningApp.WaitForElement(q => q.Marked("contentView"));
			RunningApp.Tap(q => q.Marked("contentView"));
			RunningApp.WaitForElement(q => q.Marked("Parent"));

			RunningApp.WaitForElement(q => q.Marked("stackLayout"));
			RunningApp.Tap(q => q.Marked("stackLayout"));
			RunningApp.WaitForElement(q => q.Marked("Parent"));

			RunningApp.WaitForElement(q => q.Marked("nontransparent"));
			RunningApp.Tap(q => q.Marked("nontransparent"));

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
		}
#endif
	}
}