using System.Collections;
using System.Linq;
using NUnit.Framework;


namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class StackLayoutUnitTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void EmptyLayoutDoesntCrash()
		{
			var stackLayout = new StackLayout();
			stackLayout.Layout(new Rectangle(0, 0, 200, 200));
		}

		[Test]
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

			var r = stack.GetSizeRequest(100, 100);

			Assert.AreEqual(new SizeRequest(new Size(20, 20)), r);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(new Rectangle(0, 0, 100, 20), child1.Bounds);
			Assert.AreEqual(new Rectangle(0, 26, 100, 30), child2.Bounds);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(new Rectangle(0, 0, 100, 60), child1.Bounds);
			Assert.AreEqual(new Rectangle(0, 60, 100, 40), child2.Bounds);

			stack.GetSizeRequest(100, 100);
			stack.Layout(new Rectangle(0, 0, 100, 500));

			Assert.AreEqual(new Rectangle(0, 0, 100, 460), child1.Bounds);
			Assert.AreEqual(new Rectangle(0, 460, 100, 40), child2.Bounds);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(new Rectangle(0, 0, 20, 100), child1.Bounds);
			Assert.AreEqual(new Rectangle(26, 0, 30, 100), child2.Bounds);
		}

		[Test]
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
			stack.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(new Rectangle(10, 5, 80, 20), child1.Bounds);
			Assert.AreEqual(new Rectangle(10, 31, 80, 100 - 2 * 31), child2.Bounds);
			Assert.AreEqual(new Rectangle(10, 75, 80, 20), child3.Bounds);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(new Rectangle(0, 0, 20, 100), child1.Bounds);
			Assert.AreEqual(new Rectangle(26, 0, 100 - 2 * 26, 100), child2.Bounds);
			Assert.AreEqual(new Rectangle(80, 0, 20, 100), child3.Bounds);
		}

		[Test]
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

			var size = stack.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request;
			Assert.AreEqual(new Size(30, 56), size);
		}

		[Test]
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

			var size = stack.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request;
			Assert.AreEqual(new Size(56, 30), size);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(new Rectangle(0, 0, 20, 30), stack.Children[0].Bounds);
			Assert.AreEqual(new Rectangle(90, 36, 20, 30), stack.Children[1].Bounds);
			Assert.AreEqual(new Rectangle(180, 72, 20, 30), stack.Children[2].Bounds);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(new Rectangle(0, 0, 20, 30), stack.Children.Cast<View>().ToArray()[0].Bounds);
			Assert.AreEqual(new Rectangle(26, 85, 20, 30), stack.Children.Cast<View>().ToArray()[1].Bounds);
			Assert.AreEqual(new Rectangle(52, 170, 20, 30), stack.Children.Cast<View>().ToArray()[2].Bounds);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 100, 250));

			Assert.That(stack.Children.ToArray()[0].Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(stack.Children.ToArray()[1].Bounds, Is.EqualTo(new Rectangle(0, 110, 100, 100)));
			Assert.That(stack.Children.ToArray()[2].Bounds, Is.EqualTo(new Rectangle(0, 220, 100, 30)));
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 250, 100));

			Assert.That(stack.Children.ToArray()[0].Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(stack.Children.ToArray()[1].Bounds, Is.EqualTo(new Rectangle(110, 0, 100, 100)));
			Assert.That(stack.Children.ToArray()[2].Bounds, Is.EqualTo(new Rectangle(220, 0, 30, 100)));
		}

		[Test]
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

			var result = stack.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);
			Assert.That(result.Minimum, Is.EqualTo(new Size(100, 230)));
		}

		[Test]
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

			var result = stack.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);
			Assert.That(result.Minimum, Is.EqualTo(new Size(230, 100)));
		}

		[Test]
		public void TestVisibility()
		{
			View child1, child2;
			var stack = new StackLayout
			{
				IsPlatformEnabled = true,
				Children = {
					(child1 = new View {IsPlatformEnabled = true}),
					(child2 = new View {IsPlatformEnabled = true})
				}
			};

			stack.Layout(new Rectangle(0, 0, 100, 100));

			var size = stack.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request;
			Assert.AreEqual(new Rectangle(0, 0, 100, 20), child1.Bounds);
			Assert.AreEqual(new Rectangle(0, 26, 100, 20), child2.Bounds);
			Assert.AreEqual(new Size(100, 46), size);

			child1.IsVisible = false;
			Assert.False(child1.IsVisible);
			Assert.AreEqual(new Rectangle(0, 0, 100, 20), child2.Bounds);
			size = stack.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request;
			Assert.AreEqual(new Size(100, 20), size);

		}

		[Test]
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

			var result = stack.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);
			Assert.AreEqual(new Size(10, 10), result.Minimum);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 70, 70));
			Assert.AreEqual(new Rectangle(0, 0, 70, 70), stack.Bounds);
			Assert.AreEqual(new Rectangle(0, 0, 35, 70), child1.Bounds);
			Assert.AreEqual(new Rectangle(35, 0, 35, 70), child2.Bounds);
		}

		[TestCase(StackOrientation.Vertical, LayoutAlignment.Start, false, 0, 0, 200, 100, 0, 100, 200, 10)]
		[TestCase(StackOrientation.Vertical, LayoutAlignment.Start, true, 0, 0, 200, 100, 0, 190, 200, 10)]
		[TestCase(StackOrientation.Vertical, LayoutAlignment.Center, false, 0, 0, 200, 100, 0, 100, 200, 10)]
		[TestCase(StackOrientation.Vertical, LayoutAlignment.Center, true, 0, 45, 200, 100, 0, 190, 200, 10)]
		[TestCase(StackOrientation.Vertical, LayoutAlignment.End, false, 0, 0, 200, 100, 0, 100, 200, 10)]
		[TestCase(StackOrientation.Vertical, LayoutAlignment.End, true, 0, 90, 200, 100, 0, 190, 200, 10)]
		[TestCase(StackOrientation.Horizontal, LayoutAlignment.Start, false, 0, 0, 100, 200, 100, 0, 10, 200)]
		[TestCase(StackOrientation.Horizontal, LayoutAlignment.Start, true, 0, 0, 100, 200, 190, 0, 10, 200)]
		[TestCase(StackOrientation.Horizontal, LayoutAlignment.Center, false, 0, 0, 100, 200, 100, 0, 10, 200)]
		[TestCase(StackOrientation.Horizontal, LayoutAlignment.Center, true, 45, 0, 100, 200, 190, 0, 10, 200)]
		[TestCase(StackOrientation.Horizontal, LayoutAlignment.End, false, 0, 0, 100, 200, 100, 0, 10, 200)]
		[TestCase(StackOrientation.Horizontal, LayoutAlignment.End, true, 90, 0, 100, 200, 190, 0, 10, 200)]
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

			stack.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(new Rectangle(x1, y1, w1, h1), child1.Bounds);
			Assert.AreEqual(new Rectangle(x2, y2, w2, h2), child2.Bounds);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(new Rectangle(0, 0, 200, 40), child1.Bounds);
			Assert.AreEqual(new Rectangle(0, 46, 200, 40), child2.Bounds);

			stack.Children.RemoveAt(0);

			Assert.AreEqual(new Rectangle(0, 0, 200, 40), child2.Bounds);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(LayoutConstraint.Fixed, child2.Constraint);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(LayoutConstraint.HorizontallyFixed, child2.Constraint);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(LayoutConstraint.Fixed, child2.Constraint);
		}

		[Test]
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

			stack.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(LayoutConstraint.VerticallyFixed, child2.Constraint);
		}

		[Ignore("This test intended to test bz38416 however I just for the life of me cant figure it out in simplified form. I am failure.")]
		[Test]
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

			stack.Layout(new Rectangle(0, 0, 100, 100));
		}

		[Test]
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

			var outterLayout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				IsPlatformEnabled = true,
				Children = { innerStack }
			};

			outterLayout.Layout(new Rectangle(0, 0, 100, 100));
			var beforeSize = innerStack.Bounds.Size;
			innerStack.Padding = new Thickness(30);
			var afterSize = innerStack.Bounds.Size;
			Assert.AreNotEqual(beforeSize, afterSize, "Padding was grow, so Size should be bigger");
		}

		[Test]
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

			outterLayout.Layout(new Rectangle(0, 0, 100, 100));
			var before = child.Bounds;
			innerStack.Padding = new Thickness(30);
			var after = child.Bounds;
			Assert.AreNotEqual(before, after, "child should be moved within padding size");
		}
	}
}