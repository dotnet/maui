using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3000, "Horizontal ScrollView breaks scrolling when flowdirection is set to rtl")]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ScrollView)]
#endif
	public class Issue3000 : TestContentPage
	{
		const string kSuccess = "Success";

		protected override void Init()
		{
			ScrollView view = new ScrollView();
			StackLayout parent = new StackLayout();
			Label instructions = new Label() { Text = "Scroll X should not be zero Scroll Y should be zero" };
			Label scrollPositions = new Label();
			Label outcome = new Label();

			parent.Children.Add(instructions);
			parent.Children.Add(scrollPositions);
			parent.Children.Add(outcome);

			view.Scrolled += (_, __) =>
			{
				if (outcome.Text == kSuccess)
				{
					return;
				}

				scrollPositions.Text = $"ScrollX: {view.ScrollX} ScrollY: {view.ScrollY}";
				if (view.ScrollY == 0 && view.ScrollX > 0)
				{
					outcome.Text = kSuccess;
				}
				else
				{
					outcome.Text = "Fail";
				}
			};

			view.Orientation = ScrollOrientation.Both;

			StackLayout layout = new StackLayout();
			layout.Orientation = StackOrientation.Horizontal;
			layout.Children.Add(new Label() { Text = "LEFT" });
			for (int i = 0; i < 80; i++)
				layout.Children.Add(new Image() { BackgroundColor = Color.Pink, Source = "coffee.png" });
			layout.Children.Add(new Label() { Text = "RIGHT" });



			StackLayout layoutDown = new StackLayout();
			for (int i = 0; i < 80; i++)
				layoutDown.Children.Add(new Image() { BackgroundColor = Color.Pink, Source = "coffee.png" });

			view.FlowDirection = FlowDirection.RightToLeft;
			parent.Children.Insert(0, new Button()
			{
				Text = "click me please",
				Command = new Command(() =>
				{
					if (view.FlowDirection == FlowDirection.LeftToRight)
					{
						view.FlowDirection = FlowDirection.RightToLeft;
					}
					else
					{
						view.FlowDirection = FlowDirection.LeftToRight;
					}
				})
			});

			parent.Children.Insert(0, new Button()
			{
				Text = "reset this view",
				Command = new Command(() =>
				{
					Application.Current.MainPage = new Issue3000();
				})
			});

			parent.Children.Insert(0, new Label()
			{
				Text = "right to left text",
			});

			parent.Children.Insert(0, new Label()
			{
				Text = "left to right text"
			});

			view.Content = new StackLayout()
			{
				Children =
				{
					layout, layoutDown
				}
			};

			parent.Children.Add(view);
			Content = parent;
		}


#if UITEST
		[Test]
		public void RtlScrollViewStartsScrollToRight()
		{
			RunningApp.WaitForElement(kSuccess);
		}
#endif

	}
}
