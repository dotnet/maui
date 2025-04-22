using System.Collections;
using System.Linq;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;


	public class StackLayoutUnitTests : BaseTestFixture
	{
		[Fact]
		public void EmptyLayoutDoesntCrash()
		{
			var stackLayout = new StackLayout();
			stackLayout.Layout(new Rect(0, 0, 200, 200));
		}

		[Fact]
		public void TestLastChildNotVisible()
		{
			View child1, child2;
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Children = {
					(child1 = new View {WidthRequest = 20, HeightRequest = 20, IsPlatformEnabled = true}),
					(child2 = new View {WidthRequest = 30, HeightRequest = 30, IsPlatformEnabled = true, IsVisible = false})
				}
			};

			var r = stack.Measure(100, 100, MeasureFlags.None);

			Assert.Equal(new SizeRequest(new Size(20, 20)), r);
		}

		[Fact]
		public void TestLayoutVertical()
		{
			View child1, child2;
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Children = {
					(child1 = new View {WidthRequest = 20, HeightRequest = 20, IsPlatformEnabled = true}),
					(child2 = new View {WidthRequest = 30, HeightRequest = 30, IsPlatformEnabled = true})
				}
			};

			stack.Layout(new Rect(0, 0, 100, 100));

			Assert.Equal(new Rect(0, 0, 100, 20), child1.Bounds);
			Assert.Equal(new Rect(0, 26, 100, 30), child2.Bounds);
		}

		[Fact]
		public void ReinflatesViewsCorrectly()
		{
			var child1 = new BoxView
			{
				IsPlatformEnabled = true,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HeightRequest = 400,
				MinimumHeightRequest = 10
			};

			var child2 = new BoxView
			{
				IsPlatformEnabled = true,
				HeightRequest = 40,
				MinimumHeightRequest = 40
			};

			var stack = new StackLayout
			{
				Spacing = 0,
				IsPlatformEnabled = true,
				Children = { child1, child2 }
			};

			stack.Layout(new Rect(0, 0, 100, 100));

			Assert.Equal(new Rect(0, 0, 100, 60), child1.Bounds);
			Assert.Equal(new Rect(0, 60, 100, 40), child2.Bounds);

			stack.Measure(100, 100);
			stack.Layout(new Rect(0, 0, 100, 500));

			Assert.Equal(new Rect(0, 0, 100, 460), child1.Bounds);
			Assert.Equal(new Rect(0, 460, 100, 40), child2.Bounds);
		}

		[Fact]
		public void TestLayoutHorizontal()
		{
			View child1, child2;
			var stack = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				IsPlatformEnabled = true,
				Children = {
					(child1 = new View {WidthRequest = 20, HeightRequest = 20, IsPlatformEnabled = true}),
					(child2 = new View {WidthRequest = 30, HeightRequest = 30, IsPlatformEnabled = true})
				}
			};

			stack.Layout(new Rect(0, 0, 100, 100));

			Assert.Equal(new Rect(0, 0, 20, 100), child1.Bounds);
			Assert.Equal(new Rect(26, 0, 30, 100), child2.Bounds);
		}

		[Fact]
		public void TestExpandVertical()
		{
			View child1, child2, child3;
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Orientation = StackOrientation.Vertical,
				Children = {
					(child1 = new View {WidthRequest = 20, HeightRequest = 20, IsPlatformEnabled = true}),
					(child2 = new View {
						 WidthRequest = 20,
						 HeightRequest = 20,
						 IsPlatformEnabled = true,
						 VerticalOptions = LayoutOptions.FillAndExpand
					 }),
					(child3 = new View {WidthRequest = 20, HeightRequest = 20, IsPlatformEnabled = true})
				}
			};

			stack.Padding = new Thickness(10, 5);
			stack.Layout(new Rect(0, 0, 100, 100));

			Assert.Equal(new Rect(10, 5, 80, 20), child1.Bounds);
			Assert.Equal(new Rect(10, 31, 80, 100 - 2 * 31), child2.Bounds);
			Assert.Equal(new Rect(10, 75, 80, 20), child3.Bounds);
		}

		[Fact]
		public void TestExpandHorizontal()
		{
			View child1, child2, child3;
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Orientation = StackOrientation.Horizontal,
				Children = {
					(child1 = new View {WidthRequest = 20, HeightRequest = 20, IsPlatformEnabled = true}),
					(child2 = new View {
						 WidthRequest = 20,
						 HeightRequest = 20,
						 IsPlatformEnabled = true,
						 HorizontalOptions = LayoutOptions.FillAndExpand
					 }),
					(child3 = new View {WidthRequest = 20, HeightRequest = 20, IsPlatformEnabled = true})
				}
			};

			stack.Layout(new Rect(0, 0, 100, 100));

			Assert.Equal(new Rect(0, 0, 20, 100), child1.Bounds);
			Assert.Equal(new Rect(26, 0, 100 - 2 * 26, 100), child2.Bounds);
			Assert.Equal(new Rect(80, 0, 20, 100), child3.Bounds);
		}

		[Fact]
		public void TestSizeRequestVertical()
		{
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Children = {
					new View {WidthRequest = 20, HeightRequest = 20, IsPlatformEnabled = true},
					new View {WidthRequest = 30, HeightRequest = 30, IsPlatformEnabled = true}
				}
			};

			var size = stack.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request;
			Assert.Equal(new Size(30, 56), size);
		}

		[Fact]
		public void TestSizeRequestHorizontal()
		{
			var stack = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				IsPlatformEnabled = true,
				Children = {
					new View {WidthRequest = 20, HeightRequest = 20, IsPlatformEnabled = true},
					new View {WidthRequest = 30, HeightRequest = 30, IsPlatformEnabled = true}
				}
			};

			var size = stack.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request;
			Assert.Equal(new Size(56, 30), size);
		}

		[Fact]
		public void HorizontalRequestInVerticalLayout()
		{
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Children = {
					new View {WidthRequest = 20, HeightRequest = 30, IsPlatformEnabled = true, HorizontalOptions = LayoutOptions.Start},
					new View {
						WidthRequest = 20,
						HeightRequest = 30,
						IsPlatformEnabled = true,
						HorizontalOptions = LayoutOptions.Center
					},
					new View {WidthRequest = 20, HeightRequest = 30, IsPlatformEnabled = true, HorizontalOptions = LayoutOptions.End}
				}
			};

			stack.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(0, 0, 20, 30), stack.Children[0].Frame);
			Assert.Equal(new Rect(90, 36, 20, 30), stack.Children[1].Frame);
			Assert.Equal(new Rect(180, 72, 20, 30), stack.Children[2].Frame);
		}

		[Fact]
		public void VerticalRequestInHorizontalLayout()
		{
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Orientation = StackOrientation.Horizontal,
				Children = {
					new View {WidthRequest = 20, HeightRequest = 30, IsPlatformEnabled = true, VerticalOptions = LayoutOptions.Start},
					new View {WidthRequest = 20, HeightRequest = 30, IsPlatformEnabled = true, VerticalOptions = LayoutOptions.Center},
					new View {WidthRequest = 20, HeightRequest = 30, IsPlatformEnabled = true, VerticalOptions = LayoutOptions.End}
				}
			};

			stack.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(0, 0, 20, 30), stack.Children.Cast<View>().ToArray()[0].Bounds);
			Assert.Equal(new Rect(26, 85, 20, 30), stack.Children.Cast<View>().ToArray()[1].Bounds);
			Assert.Equal(new Rect(52, 170, 20, 30), stack.Children.Cast<View>().ToArray()[2].Bounds);
		}

		[Fact]
		public void RespectMinimumHeightRequest()
		{
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Spacing = 10,
				Children = {
					new View {WidthRequest = 100, HeightRequest = 100, IsPlatformEnabled = true},
					new View {WidthRequest = 100, HeightRequest = 100, IsPlatformEnabled = true},
					new View {WidthRequest = 100, HeightRequest = 100, MinimumHeightRequest = 10, IsPlatformEnabled = true}
				}
			};

			stack.Layout(new Rect(0, 0, 100, 250));

			Assert.Equal(stack.Children.ToArray()[0].Frame, new Rect(0, 0, 100, 100));
			Assert.Equal(stack.Children.ToArray()[1].Frame, new Rect(0, 110, 100, 100));
			Assert.Equal(stack.Children.ToArray()[2].Frame, new Rect(0, 220, 100, 30));
		}

		[Fact]
		public void RespectMinimumWidthRequest()
		{
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Orientation = StackOrientation.Horizontal,
				Spacing = 10,
				Children = {
					new View {WidthRequest = 100, HeightRequest = 100, IsPlatformEnabled = true},
					new View {WidthRequest = 100, HeightRequest = 100, IsPlatformEnabled = true},
					new View {WidthRequest = 100, HeightRequest = 100, MinimumWidthRequest = 10, IsPlatformEnabled = true}
				}
			};

			stack.Layout(new Rect(0, 0, 250, 100));

			Assert.Equal(stack.Children.ToArray()[0].Frame, new Rect(0, 0, 100, 100));
			Assert.Equal(stack.Children.ToArray()[1].Frame, new Rect(110, 0, 100, 100));
			Assert.Equal(stack.Children.ToArray()[2].Frame, new Rect(220, 0, 30, 100));
		}

		[Fact]
		public void GetMinimumSizeVertical()
		{
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Spacing = 10,
				Children = {
					new View {WidthRequest = 100, HeightRequest = 100, IsPlatformEnabled = true},
					new View {WidthRequest = 100, HeightRequest = 100, IsPlatformEnabled = true},
					new View {WidthRequest = 100, HeightRequest = 100, MinimumHeightRequest = 10, IsPlatformEnabled = true}
				}
			};

			var result = stack.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
			Assert.Equal(result.Minimum, new Size(100, 230));
		}

		[Fact]
		public void GetMinimumSizeHorizontal()
		{
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Spacing = 10,
				Orientation = StackOrientation.Horizontal,
				Children = {
					new View {WidthRequest = 100, HeightRequest = 100, IsPlatformEnabled = true},
					new View {WidthRequest = 100, HeightRequest = 100, IsPlatformEnabled = true},
					new View {WidthRequest = 100, HeightRequest = 100, MinimumWidthRequest = 10, IsPlatformEnabled = true}
				}
			};

			var result = stack.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
			Assert.Equal(result.Minimum, new Size(230, 100));
		}

		[Fact]
		public void TestVisibility()
		{
			View child1, child2;
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Children = {
					(child1 = MockPlatformSizeService.Sub<View>()),
					(child2 = MockPlatformSizeService.Sub<View>())
				}
			};

			var handler = Substitute.For<IViewHandler>();
			child1.Handler = handler;

			stack.Layout(new Rect(0, 0, 100, 100));

			var size = stack.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request;
			Assert.Equal(new Rect(0, 0, 100, 20), child1.Bounds);
			Assert.Equal(new Rect(0, 26, 100, 20), child2.Bounds);
			Assert.Equal(new Size(100, 46), size);

			child1.IsVisible = false;

			// Verify that the visibility change invalidated the child
			// which will propagate the invalidation up through the platform tree.
			AssertInvalidated(handler);

			// Then simulate a native layout update 
			stack.ForceLayout();

			Assert.False(child1.IsVisible);
			Assert.Equal(new Rect(0, 0, 100, 20), child2.Bounds);
			size = stack.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request;
			Assert.Equal(new Size(100, 20), size);
		}

		[Fact]
		public void OffOrientationMinimumSize()
		{
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Spacing = 0
			};

			stack.Children.Add(new View
			{
				IsPlatformEnabled = true,
				WidthRequest = 100,
				HeightRequest = 100,
				MinimumWidthRequest = 10,
				MinimumHeightRequest = 10
			});

			var result = stack.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
			Assert.Equal(new Size(10, 10), result.Minimum);
		}

		[Fact]
		public void NestedMinimumSizeOverflow()
		{
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Spacing = 0
			};

			var hbox = new StackLayout
			{
				IsPlatformEnabled = true,
				Spacing = 0,
				Orientation = StackOrientation.Horizontal
			};

			View child1, child2;
			hbox.Children.Add(child1 = new View
			{
				IsPlatformEnabled = true,
				WidthRequest = 100,
				HeightRequest = 100,
				MinimumWidthRequest = 10,
				MinimumHeightRequest = 10
			});

			hbox.Children.Add(child2 = new View
			{
				IsPlatformEnabled = true,
				WidthRequest = 100,
				HeightRequest = 100,
				MinimumWidthRequest = 10,
				MinimumHeightRequest = 10
			});

			stack.Children.Add(hbox);

			stack.Layout(new Rect(0, 0, 70, 70));
			Assert.Equal(new Rect(0, 0, 70, 70), stack.Bounds);
			Assert.Equal(new Rect(0, 0, 35, 70), child1.Bounds);
			Assert.Equal(new Rect(35, 0, 35, 70), child2.Bounds);
		}

		[Theory]
		[InlineData(StackOrientation.Vertical, LayoutAlignment.Start, false, 0, 0, 200, 100, 0, 100, 200, 10)]
		[InlineData(StackOrientation.Vertical, LayoutAlignment.Start, true, 0, 0, 200, 100, 0, 190, 200, 10)]
		[InlineData(StackOrientation.Vertical, LayoutAlignment.Center, false, 0, 0, 200, 100, 0, 100, 200, 10)]
		[InlineData(StackOrientation.Vertical, LayoutAlignment.Center, true, 0, 45, 200, 100, 0, 190, 200, 10)]
		[InlineData(StackOrientation.Vertical, LayoutAlignment.End, false, 0, 0, 200, 100, 0, 100, 200, 10)]
		[InlineData(StackOrientation.Vertical, LayoutAlignment.End, true, 0, 90, 200, 100, 0, 190, 200, 10)]
		[InlineData(StackOrientation.Horizontal, LayoutAlignment.Start, false, 0, 0, 100, 200, 100, 0, 10, 200)]
		[InlineData(StackOrientation.Horizontal, LayoutAlignment.Start, true, 0, 0, 100, 200, 190, 0, 10, 200)]
		[InlineData(StackOrientation.Horizontal, LayoutAlignment.Center, false, 0, 0, 100, 200, 100, 0, 10, 200)]
		[InlineData(StackOrientation.Horizontal, LayoutAlignment.Center, true, 45, 0, 100, 200, 190, 0, 10, 200)]
		[InlineData(StackOrientation.Horizontal, LayoutAlignment.End, false, 0, 0, 100, 200, 100, 0, 10, 200)]
		[InlineData(StackOrientation.Horizontal, LayoutAlignment.End, true, 90, 0, 100, 200, 190, 0, 10, 200)]
		public void LayoutExpansion(StackOrientation orientation, LayoutAlignment align, bool expand, double x1, double y1, double w1, double h1, double x2, double y2, double w2, double h2)
		{
			var options = new LayoutOptions(align, expand);
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Spacing = 0,
				Orientation = orientation
			};

			View child1, child2;
			stack.Children.Add(child1 = new View
			{
				IsPlatformEnabled = true,
				WidthRequest = 100,
				HeightRequest = 100
			});

			if (orientation == StackOrientation.Vertical)
				child1.VerticalOptions = options;
			else
				child1.HorizontalOptions = options;

			stack.Children.Add(child2 = new View
			{
				IsPlatformEnabled = true,
				WidthRequest = 10,
				HeightRequest = 10
			});

			stack.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(x1, y1, w1, h1), child1.Bounds);
			Assert.Equal(new Rect(x2, y2, w2, h2), child2.Bounds);
		}

		[Fact]
		public void RelayoutOnRemove()
		{
			var child1 = new BoxView
			{
				IsPlatformEnabled = true,
			};

			var child2 = new BoxView
			{
				IsPlatformEnabled = true,
			};

			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Children = { child1, child2 }
			};

			stack.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(0, 0, 200, 40), child1.Bounds);
			Assert.Equal(new Rect(0, 46, 200, 40), child2.Bounds);

			stack.Children.RemoveAt(0);

			Assert.Equal(new Rect(0, 0, 200, 40), child2.Bounds);
		}

		[Fact]
		public void FixedVerticalStackFixesExpander()
		{
			var child1 = new BoxView
			{
				IsPlatformEnabled = true,
			};

			var child2 = new BoxView
			{
				IsPlatformEnabled = true,
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			var stack = new StackLayout
			{
				ComputedConstraint = LayoutConstraint.Fixed,
				IsPlatformEnabled = true,
				Children = { child1, child2 }
			};

			stack.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(LayoutConstraint.Fixed, child2.Constraint);
		}

		[Fact]
		public void HFixedVerticalStackFixesExpander()
		{
			var child1 = new BoxView
			{
				IsPlatformEnabled = true,
			};

			var child2 = new BoxView
			{
				IsPlatformEnabled = true,
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			var stack = new StackLayout
			{
				ComputedConstraint = LayoutConstraint.HorizontallyFixed,
				IsPlatformEnabled = true,
				Children = { child1, child2 }
			};

			stack.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(LayoutConstraint.HorizontallyFixed, child2.Constraint);
		}

		[Fact]
		public void FixedHorizontalStackFixesExpander()
		{
			var child1 = new BoxView
			{
				IsPlatformEnabled = true,
			};

			var child2 = new BoxView
			{
				IsPlatformEnabled = true,
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			var stack = new StackLayout
			{
				ComputedConstraint = LayoutConstraint.Fixed,
				Orientation = StackOrientation.Horizontal,
				IsPlatformEnabled = true,
				Children = { child1, child2 }
			};

			stack.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(LayoutConstraint.Fixed, child2.Constraint);
		}

		[Fact]
		public void HFixedHorizontalStackFixesExpander()
		{
			var child1 = new BoxView
			{
				IsPlatformEnabled = true,
			};

			var child2 = new BoxView
			{
				IsPlatformEnabled = true,
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			var stack = new StackLayout
			{
				ComputedConstraint = LayoutConstraint.VerticallyFixed,
				Orientation = StackOrientation.Horizontal,
				IsPlatformEnabled = true,
				Children = { child1, child2 }
			};

			stack.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(LayoutConstraint.VerticallyFixed, child2.Constraint);
		}

		[Fact(Skip = "This test intended to test bz38416 however I just for the life of me cant figure it out in simplified form. I am failure.")]
		public void TheWTFTest()
		{
			var child1 = new BoxView
			{
				IsPlatformEnabled = true,
				WidthRequest = 20,
				HeightRequest = 20,
				MinimumWidthRequest = 10,
				MinimumHeightRequest = 10,
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Children = { child1 }
			};

			stack.Layout(new Rect(0, 0, 100, 100));
		}

		[Fact]
		public void PaddingResizeTest()
		{
			var child = new BoxView
			{
				IsPlatformEnabled = true,
				WidthRequest = 20,
				HeightRequest = 20,
			};

			var innerStack = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				IsPlatformEnabled = true,
				Children = { child }
			};

			var outerLayout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				IsPlatformEnabled = true,
				Children = { innerStack }
			};


			var handler = Substitute.For<IViewHandler>();
			innerStack.Handler = handler;

			outerLayout.Layout(new Rect(0, 0, 100, 100));
			var beforeSize = innerStack.Bounds.Size;
			innerStack.Padding = new Thickness(30);

			// Verify that the padding change invalidated the inner stack
			// which will propagate the invalidation up through the platform tree.
			AssertInvalidated(handler);
			// Now simulate a native layout update 
			outerLayout.ForceLayout();

			var afterSize = innerStack.Bounds.Size;
			Assert.True(beforeSize != afterSize, "Padding was grow, so Size should be bigger");
		}

		[Fact]
		public void PaddingChildRelayoutTest()
		{
			var child = new BoxView
			{
				IsPlatformEnabled = true,
				WidthRequest = 20,
				HeightRequest = 20,
			};

			var innerStack = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				IsPlatformEnabled = true,
				Children = { child }
			};

			var outterLayout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				IsPlatformEnabled = true,
				Children = { innerStack }
			};

			outterLayout.Layout(new Rect(0, 0, 100, 100));
			var before = child.Bounds;
			innerStack.Padding = new Thickness(30);
			var after = child.Bounds;
			Assert.True(before != after, "child should be moved within padding size");
		}

		void AssertInvalidated(IViewHandler handler)
		{
			handler.Received().Invoke(Arg.Is(nameof(IView.InvalidateMeasure)), Arg.Any<object>());
			handler.ClearReceivedCalls();
		}
	}
}
