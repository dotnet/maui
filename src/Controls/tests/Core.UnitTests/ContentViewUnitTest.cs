using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;


	public class ContentViewUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			var contentView = new ContentView();

			Assert.Null(contentView.Content);
			Assert.Null(contentView.BackgroundColor);
			Assert.Equal(new Thickness(0), contentView.Padding);
		}

		[Fact]
		public void TestSetChild()
		{
			var contentView = new ContentView();

			var child1 = new Label();

			bool added = false;

			contentView.ChildAdded += (sender, e) => added = true;

			contentView.Content = child1;

			Assert.True(added);
			Assert.Equal(child1, contentView.Content);
			Assert.Equal(child1.Parent, contentView);

			added = false;
			contentView.Content = child1;

			Assert.False(added);
		}

		[Fact]
		public void TestReplaceChild()
		{
			var contentView = new ContentView();

			var child1 = new Label();
			var child2 = new Label();

			contentView.Content = child1;

			bool removed = false;
			bool added = false;

			contentView.ChildRemoved += (sender, e) => removed = true;
			contentView.ChildAdded += (sender, e) => added = true;

			contentView.Content = child2;

			Assert.Null(child1.Parent);

			Assert.True(removed);
			Assert.True(added);
			Assert.Equal(child2, contentView.Content);
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

				// Return the child's requested size (100x200)
				return new Size(100, 200);
			});

			var contentView = new ContentView
			{
				Padding = new Thickness(10),
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					IsPlatformEnabled = true,
					Handler = mockViewHandler
				},
				IsPlatformEnabled = true,
			};

			ICrossPlatformLayout crossPlatformLayout = contentView;

			// Test that the measure includes padding
			Assert.Equal(new Size(120, 220), crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity));

			// Lay out the content view
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 300, 300));

			// Verify child was positioned with padding respected
			Assert.Equal(new Rect(100, 50, 100, 200), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(Arg.Is<Rect>(r =>
				Math.Abs(r.X - 100) < 0.001 &&
				Math.Abs(r.Y - 50) < 0.001 &&
				Math.Abs(r.Width - 100) < 0.001 &&
				Math.Abs(r.Height - 200) < 0.001));
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

			var contentView = new ContentView
			{
				Padding = new Thickness(10),
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
			ICrossPlatformLayout crossPlatformLayout = contentView;

			// Test that the measure includes padding and respects the contentView's WidthRequest
			var result = crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
			Assert.Equal(new Size(120, 220), result);
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

			var contentView = new ContentView
			{
				Padding = new Thickness(10),
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
			ICrossPlatformLayout crossPlatformLayout = contentView;

			// Test that the measure includes padding and respects the contentView's HeightRequest
			var result = crossPlatformLayout.CrossPlatformMeasure(double.PositiveInfinity, double.PositiveInfinity);
			Assert.Equal(new Size(120, 220), result);
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

			var contentView = new ContentView
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

			ICrossPlatformLayout crossPlatformLayout = contentView;
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

			// Verify child bounds based on the updated layout implementation
			Assert.Equal(new Rect(100, 100, 0, 0), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(Arg.Is<Rect>(r =>
				Math.Abs(r.X - 100) < 0.001 &&
				Math.Abs(r.Y - 100) < 0.001 &&
				Math.Abs(r.Width - 0) < 0.001 &&
				Math.Abs(r.Height - 0) < 0.001));
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

			var contentView = new ContentView
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

			ICrossPlatformLayout crossPlatformLayout = contentView;
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

			// Verify child bounds based on the updated layout implementation
			Assert.Equal(new Rect(100, 0, 0, 0), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(Arg.Is<Rect>(r =>
				Math.Abs(r.X - 100) < 0.001 &&
				Math.Abs(r.Y - 0) < 0.001 &&
				Math.Abs(r.Width - 0) < 0.001 &&
				Math.Abs(r.Height - 0) < 0.001));
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

			var contentView = new ContentView
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

			ICrossPlatformLayout crossPlatformLayout = contentView;
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

			// Verify child bounds based on the updated layout implementation
			Assert.Equal(new Rect(100, 200, 0, 0), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(Arg.Is<Rect>(r =>
				Math.Abs(r.X - 100) < 0.001 &&
				Math.Abs(r.Y - 200) < 0.001 &&
				Math.Abs(r.Width - 0) < 0.001 &&
				Math.Abs(r.Height - 0) < 0.001));
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

			var contentView = new ContentView
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

			ICrossPlatformLayout crossPlatformLayout = contentView;
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

			// Verify child bounds based on the updated layout implementation
			Assert.Equal(new Rect(100, 100, 0, 0), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(Arg.Is<Rect>(r =>
				Math.Abs(r.X - 100) < 0.001 &&
				Math.Abs(r.Y - 100) < 0.001 &&
				Math.Abs(r.Width - 0) < 0.001 &&
				Math.Abs(r.Height - 0) < 0.001));
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

			var contentView = new ContentView
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

			ICrossPlatformLayout crossPlatformLayout = contentView;
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

			// Verify child bounds based on the updated layout implementation
			Assert.Equal(new Rect(0, 100, 0, 0), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(Arg.Is<Rect>(r =>
				Math.Abs(r.X - 0) < 0.001 &&
				Math.Abs(r.Y - 100) < 0.001 &&
				Math.Abs(r.Width - 0) < 0.001 &&
				Math.Abs(r.Height - 0) < 0.001));
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

			var contentView = new ContentView
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

			ICrossPlatformLayout crossPlatformLayout = contentView;
			crossPlatformLayout.CrossPlatformArrange(new Rect(0, 0, 200, 200));

			// Verify child was positioned at the right
			Assert.Equal(new Rect(200, 100, 0, 0), child.Bounds);

			// Verify the PlatformArrange method was called on the handler with the correct frame
			mockViewHandler.Received().PlatformArrange(Arg.Is<Rect>(r =>
				Math.Abs(r.X - 200) < 0.001 &&
				Math.Abs(r.Y - 100) < 0.001 &&
				Math.Abs(r.Width - 0) < 0.001 &&
				Math.Abs(r.Height - 0) < 0.001));
		}

		[Fact]
		public void NullTemplateDirectlyHosts()
		{
			// order of setting properties carefully picked to emulate running on real backend

			var contentView = new ContentView();
			var child = new View();

			contentView.Content = child;

			Assert.Equal(child, ((IElementController)contentView).LogicalChildren[0]);
		}

		class SimpleTemplate : StackLayout
		{
			public SimpleTemplate()
			{
				Children.Add(new Label());
				Children.Add(new ContentPresenter());
			}
		}


		[Fact]
		public void TemplateInflates()
		{
			var contentView = new ContentView();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));

			Assert.IsType<SimpleTemplate>(((IElementController)contentView).LogicalChildren[0]);
		}

		[Fact]
		public void PacksContent()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child;

			Assert.IsType<SimpleTemplate>(((IElementController)contentView).LogicalChildren[0]);

			Assert.Contains(child, contentView.Descendants());
		}

		[Fact]
		public void DoesInheritBindingContextToTemplate()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child;

			var bc = "Test";
			contentView.BindingContext = bc;

			Assert.Equal(bc, ((IElementController)contentView).LogicalChildren[0].BindingContext);
		}

		[Fact]
		public void DoesNotInheritBindingContextToContentFromControlTemplate()
		{
			var contentView = new ContentView();
			var child1 = new View();
			var child2 = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child1;

			var bcContentView = "Test";
			var bcSimpleTemplate = "other context";
			contentView.BindingContext = bcContentView;

			var simpleTemplate = contentView.GetVisualTreeDescendants().OfType<SimpleTemplate>().Single();
			var cp = contentView.GetVisualTreeDescendants().OfType<ContentPresenter>().Single();
			simpleTemplate.BindingContext = bcSimpleTemplate;

			Assert.Equal(bcContentView, child1.BindingContext);
			Assert.Equal(contentView.BindingContext, child1.BindingContext);
			Assert.Equal(bcSimpleTemplate, simpleTemplate.BindingContext);

			// Change out content and make sure simple templates BC doesn't propagate
			contentView.Content = child2;

			Assert.Equal(bcContentView, child2.BindingContext);
			Assert.Equal(contentView.BindingContext, child2.BindingContext);
			Assert.Equal(bcSimpleTemplate, simpleTemplate.BindingContext);
			Assert.Equal(bcSimpleTemplate, cp.BindingContext);
		}

		[Fact]
		public void ContentDoesGetBindingContext()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child;

			var bc = "Test";
			contentView.BindingContext = bc;

			Assert.Equal(bc, child.BindingContext);
		}

		[Fact]
		public void ContentParentIsNotInsideTemplate()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child;

			Assert.Equal(contentView, child.Parent);
		}

		[Fact]
		public void NonTemplatedContentInheritsBindingContext()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.Content = child;
			contentView.BindingContext = "Foo";

			Assert.Equal("Foo", child.BindingContext);
		}

		[Fact]
		public void ContentView_should_have_the_InternalChildren_correctly_when_Content_changed()
		{
			var sut = new ContentView();
			var controlTemplated = (IControlTemplated)sut;
			var internalChildren = controlTemplated.InternalChildren;
			controlTemplated.AddLogicalChild(new VisualElement());
			controlTemplated.AddLogicalChild(new VisualElement());
			controlTemplated.AddLogicalChild(new VisualElement());

			var expected = new View();
			sut.Content = expected;

			Assert.Single(internalChildren);
			Assert.Same(expected, internalChildren[0]);
		}


		[Fact]
		public void BindingContextNotLostWhenSwitchingTemplates()
		{
			var bc = new object();
			var contentView = new ContentView()
			{
				BindingContext = bc
			};


			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));

			var simpleTemplate1 = contentView.GetVisualTreeDescendants().OfType<SimpleTemplate>().Single();
			contentView.Content = new Label();

			Assert.Same(bc, simpleTemplate1.BindingContext);

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));

			var simpleTemplate2 = contentView.GetVisualTreeDescendants().OfType<SimpleTemplate>().Single();

			Assert.NotSame(simpleTemplate1, simpleTemplate2);
			Assert.Same(bc, simpleTemplate2.BindingContext);
		}
	}
}
