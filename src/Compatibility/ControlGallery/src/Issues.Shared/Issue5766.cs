using System;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	using IApp = Xamarin.UITest.IApp;
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5766, "Frame size gets corrupted when ListView is scrolled", PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Layout)]
#endif
	public class Issue5766 : TestContentPage
	{
		const string StartText1 = "start1";
		const string BigText1 = "big string > big frame1";
		const string SmallText1 = "s1";
		const string EndText1 = "end1";
		const string List1 = "lst1";

		const string StartText2 = "start2";
		const string BigText2 = "big string > big frame2";
		const string SmallText2 = "s2";
		const string EndText2 = "end2";
		const string List2 = "lst2";

		protected override void Init()
		{
			var grid = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto},
					new RowDefinition(),
				}
			};
			grid.AddChild(new Label
			{
				Text = "Scroll up and down several times and make sure Frame size is accurate when using Fast Renderers.",
				VerticalTextAlignment = TextAlignment.Center
			}, 0, 0, 2);

			var template = new DataTemplate(() =>
			{
				var text = new Label
				{
					VerticalOptions = LayoutOptions.Fill,
					TextColor = Colors.White,
				};

				text.SetBinding(Label.TextProperty, ".");
				var view = new Grid
				{
					HeightRequest = 80,
					Margin = new Thickness(0, 10, 0, 0),
					BackgroundColor = Color.FromArgb("#F1F1F1")
				};
				view.AddChild(new Frame
				{
					Padding = new Thickness(5),
					Margin = new Thickness(0, 0, 10, 0),
					BorderColor = Colors.Blue,
					BackgroundColor = Colors.Gray,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.End,
					CornerRadius = 3,
					Content = text
				}, 0, 0);
				return new ViewCell
				{
					View = view
				};
			});

			grid.AddChild(new ListView
			{
				AutomationId = List1,
				HasUnevenRows = true,
				ItemsSource = (new[] { StartText1 }).Concat(Enumerable.Range(0, 99).Select(i => i % 2 != 0 ? SmallText1 : BigText1)).Concat(new[] { EndText1 }),
				ItemTemplate = template
			}, 0, 1);
			grid.AddChild(new ListView(ListViewCachingStrategy.RecycleElement)
			{
				AutomationId = List2,
				HasUnevenRows = true,
				ItemsSource = (new[] { StartText2 }).Concat(Enumerable.Range(0, 99).Select(i => i % 2 != 0 ? SmallText2 : BigText2)).Concat(new[] { EndText2 }),
				ItemTemplate = template
			}, 1, 1);
			Content = grid;
		}

#if UITEST && __ANDROID__
		Xamarin.UITest.Queries.AppRect[] GetLabels(Xamarin.UITest.IApp RunningApp, string label)
		{
			return RunningApp
				.Query(q => q.Class("FormsTextView"))
				.Where(x => x.Text == label)
				.Select(x => x.Rect)
				.ToArray();
		}

		bool RectIsEquals(Xamarin.UITest.Queries.AppRect[] left, Xamarin.UITest.Queries.AppRect[] right)
		{
			if (left.Length != right.Length)
				return false;

			for (int i = 0; i < left.Length; i++)
			{
				if (left[i].X != right[i].X || 
					left[i].Y != right[i].Y || 
					left[i].Width != right[i].Width || 
					left[i].Height != right[i].Height)
					return false;
			}

			return true;
		}

		[Test]
		[Ignore("Fails sometimes - needs a better test")]
		public void FrameSizeGetsCorruptedWhenListViewIsScrolled()
		{
			RunningApp.WaitForElement(StartText1);
			var start = GetLabels(RunningApp, StartText1);
			var smalls = GetLabels(RunningApp, SmallText1);
			var bigs = GetLabels(RunningApp, BigText1);

			RunningApp.ScrollDownTo(EndText1, List1, ScrollStrategy.Gesture, 0.9, 15000, timeout: TimeSpan.FromMinutes(1));
			RunningApp.ScrollUpTo(StartText1, List1, ScrollStrategy.Gesture, 0.9, 15000, timeout: TimeSpan.FromMinutes(1));

			var startAfter = GetLabels(RunningApp, StartText1);
			Assert.IsTrue(RectIsEquals(start, startAfter));
			var smallAfter = GetLabels(RunningApp, SmallText1);
			Assert.IsTrue(RectIsEquals(smalls, smallAfter));
			var bigAfter = GetLabels(RunningApp, BigText1);
			Assert.IsTrue(RectIsEquals(bigs, bigAfter));

			// list2 with ListViewCachingStrategy.RecycleElement - issue 6297
			RunningApp.WaitForElement(StartText2);
			start = GetLabels(RunningApp, StartText2);
			smalls = GetLabels(RunningApp, SmallText2);
			bigs = GetLabels(RunningApp, BigText2);

			RunningApp.ScrollDownTo(EndText2, List2, ScrollStrategy.Gesture, 0.9, 15000, timeout: TimeSpan.FromMinutes(1));
			RunningApp.ScrollUpTo(StartText2, List2, ScrollStrategy.Gesture, 0.9, 15000, timeout: TimeSpan.FromMinutes(1));

			startAfter = GetLabels(RunningApp, StartText2);
			Assert.IsTrue(RectIsEquals(start, startAfter));
			smallAfter = GetLabels(RunningApp, SmallText2);
			Assert.IsTrue(RectIsEquals(smalls, smallAfter));
			bigAfter = GetLabels(RunningApp, BigText2);
			Assert.IsTrue(RectIsEquals(bigs, bigAfter));
		}
#endif
	}
}
