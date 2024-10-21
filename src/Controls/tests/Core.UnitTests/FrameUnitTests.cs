using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class FrameUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			Frame frame = new Frame();

			Assert.Null(frame.Content);
			Assert.Equal(new Thickness(20, 20, 20, 20), frame.Padding);
		}

		[Fact]
		public void TestSetChild()
		{
			Frame frame = new Frame();

			var child1 = new Label();

			bool added = false;

			frame.ChildAdded += (sender, e) => added = true;

			frame.Content = child1;

			Assert.True(added);
			Assert.Equal(child1, frame.Content);
			Assert.Equal(child1.Parent, frame);

			added = false;
			frame.Content = child1;

			Assert.False(added);
		}

		[Fact]
		public void TestReplaceChild()
		{
			Frame frame = new Frame();

			var child1 = new Label();
			var child2 = new Label();

			frame.Content = child1;

			bool removed = false;
			bool added = false;

			frame.ChildRemoved += (sender, e) => removed = true;
			frame.ChildAdded += (sender, e) => added = true;

			frame.Content = child2;
			Assert.Null(child1.Parent);

			Assert.True(removed);
			Assert.True(added);
			Assert.Equal(child2, frame.Content);
		}

		[Fact]
		public void TestFrameLayout()
		{
			View child;

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					IsPlatformEnabled = true
				},
				IsPlatformEnabled = true,
			};

			Assert.Equal(new Size(140, 240), frame.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request);

			frame.Layout(new Rect(0, 0, 300, 300));

			Assert.Equal(new Rect(20, 20, 260, 260), child.Bounds);
		}

		[Fact]
		public void TestDoesNotThrowOnSetNullChild()
		{
			_ = new Frame { Content = null };
		}

		[Fact]
		public void WidthRequest()
		{
			var frame = new Frame
			{
				Content = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					IsPlatformEnabled = true
				},
				IsPlatformEnabled = true,
				WidthRequest = 20
			};

			Assert.Equal(new Size(60, 240), frame.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request);
		}

		[Fact]
		public void HeightRequest()
		{
			var frame = new Frame
			{
				Content = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					IsPlatformEnabled = true,
				},
				IsPlatformEnabled = true,
				HeightRequest = 20
			};

			Assert.Equal(new Size(140, 60), frame.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request);
		}

		[Fact]
		public void LayoutVerticallyCenter()
		{
			View child;

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					VerticalOptions = LayoutOptions.Center
				},
				IsPlatformEnabled = true,
			};

			frame.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(20, 50, 160, 100), child.Bounds);
		}

		[Fact]
		public void LayoutVerticallyBegin()
		{
			View child;

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					VerticalOptions = LayoutOptions.Start
				},
				IsPlatformEnabled = true,
			};

			frame.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(20, 20, 160, 100), child.Bounds);
		}

		[Fact]
		public void LayoutVerticallyEnd()
		{
			View child;

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					VerticalOptions = LayoutOptions.End
				},
				IsPlatformEnabled = true,
			};

			frame.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(20, 80, 160, 100), child.Bounds);
		}

		[Fact]
		public void LayoutHorizontallyCenter()
		{
			View child;

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					HorizontalOptions = LayoutOptions.Center
				},
				IsPlatformEnabled = true,
			};

			frame.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(50, 20, 100, 160), child.Bounds);
		}

		[Fact]
		public void LayoutHorizontallyBegin()
		{
			View child;

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					HorizontalOptions = LayoutOptions.Start
				},
				IsPlatformEnabled = true,
			};

			frame.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(20, 20, 100, 160), child.Bounds);
		}

		[Fact]
		public void LayoutHorizontallyEnd()
		{
			View child;

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					HorizontalOptions = LayoutOptions.End
				},
				IsPlatformEnabled = true,
			};

			frame.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(80, 20, 100, 160), child.Bounds);
		}

		[Fact]
		public void SettingPaddingThroughStyle()
		{
			var frame = new Frame
			{
				Style = new Style(typeof(Frame))
				{
					Setters = {
						new Setter {Property = Layout.PaddingProperty, Value = 0}
					}
				}
			};

			Assert.Equal(new Thickness(0), frame.Padding);

		}
	}
}