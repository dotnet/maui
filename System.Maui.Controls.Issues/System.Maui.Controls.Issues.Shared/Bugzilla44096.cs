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
	[Issue(IssueTracker.Bugzilla, 44096, "Grid, StackLayout, and ContentView still participate in hit testing on " 
		+ "Android after IsEnabled is set to false", PlatformAffected.Android)]
	public class Bugzilla44096 : TestContentPage
	{
		bool _flag;
		const string Child = "Child";
		const string Original = "Original";
		const string ToggleColor = "color";
		const string ToggleIsEnabled = "disabled";

		const string StackLayout = "stackLayout";
		const string ContentView = "contentView";
		const string Grid = "grid";
		const string RelativeLayout = "relativeLayout";

		protected override void Init()
		{
			var result = new Label
			{
				Text = Original
			};

			var grid = new Grid
			{
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = Grid
			};
			AddTapGesture(result, grid);

			var contentView = new ContentView
			{
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = ContentView
			};
			AddTapGesture(result, contentView);

			var stackLayout = new StackLayout
			{
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = StackLayout
			};
			AddTapGesture(result, stackLayout);

			var relativeLayout = new RelativeLayout
			{
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = RelativeLayout
			};
			AddTapGesture(result, relativeLayout);

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
						relativeLayout.BackgroundColor = Color.Green;
					}
					else
					{
						grid.BackgroundColor = Color.Default;
						contentView.BackgroundColor = Color.Default;
						stackLayout.BackgroundColor = Color.Default;
						relativeLayout.BackgroundColor = Color.Default;
					}

					_flag = !_flag;
				}),
				AutomationId = ToggleColor
			};

			var disabled = new Button
			{
				Text = "Toggle IsEnabled",
				Command = new Command(() =>
				{
					grid.IsEnabled = false;
					contentView.IsEnabled = false;
					stackLayout.IsEnabled = false;
					relativeLayout.IsEnabled = false;

					result.Text = Original;
				}),
				AutomationId = ToggleIsEnabled
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
					stackLayout,
					relativeLayout
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
					result.Text = Child;
				})
			};
			view.GestureRecognizers.Add(tapGestureRecognizer);
		}

#if UITEST

		[Test]
		public void TestGrid()
		{
			TestControl(Grid);
		}

		[Test]
		public void TestContentView()
		{
			TestControl(ContentView);
		}

		[Test]
		public void TestStackLayout()
		{
			TestControl(StackLayout);
		}

		[Test]
		public void TestRelativeLayout()
		{
			TestControl(RelativeLayout);
		}

		void TestControl(string control)
		{
			RunningApp.WaitForElement(q => q.Marked(control));
			RunningApp.Tap(q => q.Marked(control));
			RunningApp.WaitForElement(q => q.Marked(Child));

			RunningApp.WaitForElement(q => q.Marked(ToggleColor));
			RunningApp.Tap(q => q.Marked(ToggleColor));

			RunningApp.WaitForElement(q => q.Marked(control));
			RunningApp.Tap(q => q.Marked(control));
			RunningApp.WaitForElement(q => q.Marked(Child));

			RunningApp.WaitForElement(q => q.Marked(ToggleIsEnabled));
			RunningApp.Tap(q => q.Marked(ToggleIsEnabled));

			RunningApp.WaitForElement(q => q.Marked(control));
			RunningApp.Tap(q => q.Marked(control));
			RunningApp.WaitForElement(q => q.Marked(Original));

			RunningApp.WaitForElement(q => q.Marked(ToggleColor));
			RunningApp.Tap(q => q.Marked(ToggleColor));

			RunningApp.WaitForElement(q => q.Marked(control));
			RunningApp.Tap(q => q.Marked(control));
			RunningApp.WaitForElement(q => q.Marked(Original));
		}
#endif
	}
}