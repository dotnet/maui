using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using NSubstitute;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

	[TestFixture]
	public class ScrollViewUnitTests : BaseTestFixture
	{
		[Test]
		public void TestConstructor()
		{
			ScrollView scrollView = new ScrollView();

			Assert.Null(scrollView.Content);

			View view = new View();
			scrollView = new ScrollView { Content = view };

			Assert.AreEqual(view, scrollView.Content);
		}

		[Test]
		[TestCase(ScrollOrientation.Horizontal)]
		[TestCase(ScrollOrientation.Both)]
		public void GetsCorrectSizeRequestWithWrappingContent(ScrollOrientation orientation)
		{
			MockPlatformSizeService.Current.UseRealisticLabelMeasure = true;

			var scrollView = new ScrollView
			{
				IsPlatformEnabled = true,
				Orientation = orientation,
			};

			var hLayout = new StackLayout
			{
				IsPlatformEnabled = true,
				Orientation = StackOrientation.Horizontal,
				Children = {
					new Label {Text = "THIS IS A REALLY LONG STRING", IsPlatformEnabled = true},
					new Label {Text = "THIS IS A REALLY LONG STRING", IsPlatformEnabled = true},
					new Label {Text = "THIS IS A REALLY LONG STRING", IsPlatformEnabled = true},
					new Label {Text = "THIS IS A REALLY LONG STRING", IsPlatformEnabled = true},
					new Label {Text = "THIS IS A REALLY LONG STRING", IsPlatformEnabled = true},
				}
			};

			scrollView.Content = hLayout;

			var r = scrollView.Measure(100, 100);

			Assert.AreEqual(10, r.Request.Height);
		}

		[Test]
		public void TestChildChanged()
		{
			ScrollView scrollView = new ScrollView();

			bool changed = false;
			scrollView.PropertyChanged += (sender, e) =>
			{
				switch (e.PropertyName)
				{
					case "Content":
						changed = true;
						break;
				}
			};
			View view = new View();
			scrollView.Content = view;

			Assert.True(changed);
		}

		[Test]
		public void TestChildDoubleSet()
		{
			var scrollView = new ScrollView();

			bool changed = false;
			scrollView.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Content")
					changed = true;
			};

			var child = new View();
			scrollView.Content = child;

			Assert.True(changed);
			Assert.AreEqual(child, scrollView.Content);
			Assert.AreEqual(child.Parent, scrollView);

			changed = false;

			scrollView.Content = child;

			Assert.False(changed);

			scrollView.Content = null;

			Assert.True(changed);
			Assert.Null(scrollView.Content);
			Assert.Null(child.Parent);
		}

		[Test]
		public void TestOrientation()
		{
			var scrollView = new ScrollView();

			Assert.AreEqual(ScrollOrientation.Vertical, scrollView.Orientation);

			bool signaled = false;
			scrollView.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Orientation")
					signaled = true;
			};

			scrollView.Orientation = ScrollOrientation.Horizontal;

			Assert.AreEqual(ScrollOrientation.Horizontal, scrollView.Orientation);
			Assert.True(signaled);

			scrollView.Orientation = ScrollOrientation.Both;
			Assert.AreEqual(ScrollOrientation.Both, scrollView.Orientation);
			Assert.True(signaled);

			scrollView.Orientation = ScrollOrientation.Neither;
			Assert.AreEqual(ScrollOrientation.Neither, scrollView.Orientation);
			Assert.True(signaled);
		}

		[Test]
		public void TestOrientationDoubleSet()
		{
			var scrollView = new ScrollView();

			bool signaled = false;
			scrollView.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Orientation")
					signaled = true;
			};

			scrollView.Orientation = scrollView.Orientation;

			Assert.False(signaled);
		}


		[Test]
		public void TestScrollTo()
		{
			var scrollView = new ScrollView();

			var item = new View { };
			scrollView.Content = new StackLayout { Children = { item } };

			bool requested = false;
			((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
			{
				requested = true;
				Assert.AreEqual(args.ScrollY, 100);
				Assert.AreEqual(args.ScrollX, 0);
				Assert.Null(args.Item);
				Assert.That(args.ShouldAnimate, Is.EqualTo(true));
			};

			scrollView.ScrollToAsync(0, 100, true);
			Assert.That(requested, Is.True);
		}

		[Test]
		public void TestScrollWasNotFiredOnNeither()
		{
			var scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Neither
			};

			var item = new View { };
			scrollView.Content = new StackLayout { Children = { item } };

			bool requested = false;
			((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
			{
				requested = true;
			};

			scrollView.ScrollToAsync(0, 100, true);
			Assert.That(requested, Is.False);
		}

		[Test]
		public void TestScrollToNotAnimated()
		{
			var scrollView = new ScrollView();

			var item = new View { };
			scrollView.Content = new StackLayout { Children = { item } };

			bool requested = false;
			((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
			{
				requested = true;
				Assert.AreEqual(args.ScrollY, 100);
				Assert.AreEqual(args.ScrollX, 0);
				Assert.Null(args.Item);
				Assert.That(args.ShouldAnimate, Is.EqualTo(false));
			};

			scrollView.ScrollToAsync(0, 100, false);
			Assert.That(requested, Is.True);
		}

		[Test]
		public void TestScrollToElement()
		{
			var scrollView = new ScrollView();

			var item = new Label { Text = "Test" };
			scrollView.Content = new StackLayout { Children = { item } };

			bool requested = false;
			((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
			{
				requested = true;

				Assert.That(args.Element, Is.SameAs(item));
				Assert.That(args.Position, Is.EqualTo(ScrollToPosition.Center));
				Assert.That(args.ShouldAnimate, Is.EqualTo(true));
			};

			scrollView.ScrollToAsync(item, ScrollToPosition.Center, true);
			Assert.That(requested, Is.True);
		}

		[Test]
		public void TestScrollToElementNotAnimated()
		{
			var scrollView = new ScrollView();

			var item = new Label { Text = "Test" };
			scrollView.Content = new StackLayout { Children = { item } };

			bool requested = false;
			((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
			{
				requested = true;

				Assert.That(args.Element, Is.SameAs(item));
				Assert.That(args.Position, Is.EqualTo(ScrollToPosition.Center));
				Assert.That(args.ShouldAnimate, Is.EqualTo(false));
			};

			scrollView.ScrollToAsync(item, ScrollToPosition.Center, false);
			Assert.That(requested, Is.True);
		}

		[Test]
		public void TestScrollToInvalid()
		{
			var scrollView = new ScrollView();

			Assert.That(() => scrollView.ScrollToAsync(new VisualElement(), ScrollToPosition.Center, true), Throws.ArgumentException);
			Assert.That(() => scrollView.ScrollToAsync(null, (ScrollToPosition)500, true), Throws.ArgumentException);
		}

		[Test]
		public void SetScrollPosition()
		{
			var scroll = new ScrollView();
			IScrollViewController controller = scroll;
			controller.SetScrolledPosition(100, 100);

			Assert.That(scroll.ScrollX, Is.EqualTo(100));
			Assert.That(scroll.ScrollY, Is.EqualTo(100));
		}

		[Test]
		public void TestBackToBackBiDirectionalScroll()
		{
			var scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Both,
				Content = new Grid
				{
					WidthRequest = 1000,
					HeightRequest = 1000
				}
			};

			var y100Count = 0;

			((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
			{
				if (args.ScrollY == 100)
				{
					++y100Count;
				}
			};

			scrollView.ScrollToAsync(100, 100, true);
			Assert.AreEqual(y100Count, 1);

			scrollView.ScrollToAsync(0, 100, true);
			Assert.AreEqual(y100Count, 2);
		}

		void AssertInvalidated(IViewHandler handler)
		{
			handler.Received().Invoke(Arg.Is(nameof(IView.InvalidateMeasure)), Arg.Any<object>());
			handler.ClearReceivedCalls();
		}
	}
}
