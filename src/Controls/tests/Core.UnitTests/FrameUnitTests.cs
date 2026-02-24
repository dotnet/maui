using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using NSubstitute;
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
			var mockViewHandler = Substitute.For<IViewHandler>();

			// Set up the mock handler to properly participate in the layout system
			mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
			{
				// Extract the constraints passed to GetDesiredSize
				var width = (double)callInfo[0];
				var height = (double)callInfo[1];

				// Return the child's requested size
				return new Size(100, 200);
			});

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					IsPlatformEnabled = true,
					Handler = mockViewHandler
				},
				IsPlatformEnabled = true,
			};

			ICrossPlatformLayout crossPlatformLayout = frame;

			Assert.Equal(new Size(140, 240), crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity));

			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 300, 300));

			Assert.Equal(new Rect(100, 50, 100, 200), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(new Rect(100, 50, 100, 200));
		}

		[Fact]
		public void TestDoesNotThrowOnSetNullChild()
		{
			_ = new Frame { Content = null };
		}

		[Fact]
		public void WidthRequest()
		{
			View child;
			var mockViewHandler = Substitute.For<IViewHandler>();

			// Set up the mock handler to properly participate in the layout system
			mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
			{
				// Extract the constraints passed to GetDesiredSize
				var width = (double)callInfo[0];
				var height = (double)callInfo[1];

				// Return the child's requested size
				return new Size(100, 200);
			});

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					IsPlatformEnabled = true,
					Handler = mockViewHandler
				},
				IsPlatformEnabled = true,
				WidthRequest = 20
			};

			// Get the ICrossPlatformLayout implementation
			ICrossPlatformLayout crossPlatformLayout = frame;

			Assert.Equal(new Size(140, 240), crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity));
		}

		[Fact]
		public void HeightRequest()
		{
			View child;
			var mockViewHandler = Substitute.For<IViewHandler>();

			// Set up the mock handler to properly participate in the layout system
			mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
			{
				// Extract the constraints passed to GetDesiredSize
				var width = (double)callInfo[0];
				var height = (double)callInfo[1];

				// Return the child's requested size
				return new Size(100, 200);
			});

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					IsPlatformEnabled = true,
					Handler = mockViewHandler
				},
				IsPlatformEnabled = true,
				HeightRequest = 20
			};

			// Get the ICrossPlatformLayout implementation
			ICrossPlatformLayout crossPlatformLayout = frame;

			Assert.Equal(new Size(140, 240), crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity));
		}

		[Fact]
		public void LayoutVerticallyCenter()
		{
			View child;
			var mockViewHandler = Substitute.For<IViewHandler>();

			// Set up the mock handler to properly participate in the layout system
			mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
			{
				// Extract the constraints passed to GetDesiredSize
				var width = (double)callInfo[0];
				var height = (double)callInfo[1];

				// Return the child's requested size
				return new Size(100, 100);
			});

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					VerticalOptions = LayoutOptions.Center,
					Handler = mockViewHandler
				},
				IsPlatformEnabled = true,
			};

			ICrossPlatformLayout crossPlatformLayout = frame;
			crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(50, 50, 100, 100), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(new Rect(50, 50, 100, 100));
		}

		[Fact]
		public void LayoutVerticallyBegin()
		{
			View child;
			var mockViewHandler = Substitute.For<IViewHandler>();

			// Set up the mock handler to properly participate in the layout system
			mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
			{
				// Extract the constraints passed to GetDesiredSize
				var width = (double)callInfo[0];
				var height = (double)callInfo[1];

				// Return the child's requested size
				return new Size(100, 100);
			});

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					VerticalOptions = LayoutOptions.Start,
					Handler = mockViewHandler
				},
				IsPlatformEnabled = true,
			};

			ICrossPlatformLayout crossPlatformLayout = frame;
			crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(50, 20, 100, 100), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(new Rect(50, 20, 100, 100));
		}

		[Fact]
		public void LayoutVerticallyEnd()
		{
			View child;
			var mockViewHandler = Substitute.For<IViewHandler>();

			// Set up the mock handler to properly participate in the layout system
			mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
			{
				// Extract the constraints passed to GetDesiredSize
				var width = (double)callInfo[0];
				var height = (double)callInfo[1];

				// Return the child's requested size
				return new Size(100, 100);
			});

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					VerticalOptions = LayoutOptions.End,
					Handler = mockViewHandler
				},
				IsPlatformEnabled = true,
			};

			ICrossPlatformLayout crossPlatformLayout = frame;
			crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(50, 80, 100, 100), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(new Rect(50, 80, 100, 100));
		}

		[Fact]
		public void LayoutHorizontallyCenter()
		{
			View child;
			var mockViewHandler = Substitute.For<IViewHandler>();

			// Set up the mock handler to properly participate in the layout system
			mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
			{
				// Extract the constraints passed to GetDesiredSize
				var width = (double)callInfo[0];
				var height = (double)callInfo[1];

				// Return the child's requested size
				return new Size(100, 100);
			});

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					HorizontalOptions = LayoutOptions.Center,
					Handler = mockViewHandler
				},
				IsPlatformEnabled = true,
			};

			ICrossPlatformLayout crossPlatformLayout = frame;
			crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(50, 50, 100, 100), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(new Rect(50, 50, 100, 100));
		}

		[Fact]
		public void LayoutHorizontallyBegin()
		{
			View child;
			var mockViewHandler = Substitute.For<IViewHandler>();

			// Set up the mock handler to properly participate in the layout system
			mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
			{
				// Extract the constraints passed to GetDesiredSize
				var width = (double)callInfo[0];
				var height = (double)callInfo[1];

				// Return the child's requested size
				return new Size(100, 100);
			});

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					HorizontalOptions = LayoutOptions.Start,
					Handler = mockViewHandler
				},
				IsPlatformEnabled = true,
			};

			ICrossPlatformLayout crossPlatformLayout = frame;
			crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(20, 50, 100, 100), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(new Rect(20, 50, 100, 100));
		}

		[Fact]
		public void LayoutHorizontallyEnd()
		{
			View child;
			var mockViewHandler = Substitute.For<IViewHandler>();

			// Set up the mock handler to properly participate in the layout system
			mockViewHandler.GetDesiredSize(Arg.Any<double>(), Arg.Any<double>()).Returns(callInfo =>
			{
				// Extract the constraints passed to GetDesiredSize
				var width = (double)callInfo[0];
				var height = (double)callInfo[1];

				// Return the child's requested size
				return new Size(100, 100);
			});

			var frame = new Frame
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					HorizontalOptions = LayoutOptions.End,
					Handler = mockViewHandler
				},
				IsPlatformEnabled = true,
			};

			ICrossPlatformLayout crossPlatformLayout = frame;
			crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(80, 50, 100, 100), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(new Rect(80, 50, 100, 100));
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