#nullable enable
using System.Collections;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[TestFixture, Category("Layout")]
	public class AndExpandTests : BaseTestFixture
	{
		const double TestAreaWidth = 640;
		const double TestAreaHeight = 480;
		const double TestViewHeight = 20;
		const double TestViewWidth = 100;

		public class TestView : Button
		{
			Size _desiredSize = new(TestViewWidth, TestViewHeight);

			protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
			{
				DesiredSize = _desiredSize;
				return _desiredSize;
			}
		}

		static StackLayout SetUpTestLayout(StackOrientation orientation, params View[] views)
		{
			var layout = new StackLayout() { Orientation = orientation };

			foreach (var view in views)
			{
				layout.Add(view);
			}

			MeasureAndArrange(layout as Maui.ILayout);

			return layout;
		}

		static void MeasureAndArrange(Maui.ILayout layout)
		{
			var layoutSize = new Size(TestAreaWidth, TestAreaHeight);
			var rect = new Rect(Point.Zero, layoutSize);

			(layout as Maui.ILayout).CrossPlatformMeasure(layoutSize.Width, layoutSize.Height);
			(layout as Maui.ILayout).CrossPlatformArrange(rect);
		}

		[Test]
		public void SingleChildExpandsToFillVertical()
		{
			var view0 = new TestView
			{
				Text = "Hello",
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			var layout = SetUpTestLayout(StackOrientation.Vertical, view0);

			Assert.AreEqual(TestAreaWidth, view0.Bounds.Width);
			Assert.AreEqual(TestAreaHeight, view0.Bounds.Height);
			Assert.AreEqual(0, view0.Bounds.X);
			Assert.AreEqual(0, view0.Bounds.Y);
		}

		[Test]
		public void SingleChildExpandsToFillHorizontal()
		{
			var view0 = new TestView
			{
				Text = "Hello",
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			var layout = SetUpTestLayout(StackOrientation.Horizontal, view0);

			Assert.AreEqual(TestAreaWidth, view0.Bounds.Width);
			Assert.AreEqual(TestAreaHeight, view0.Bounds.Height);
			Assert.AreEqual(0, view0.Bounds.X);
			Assert.AreEqual(0, view0.Bounds.Y);
		}

		[Test]
		public void TwoChildrenSplit5050Vertical()
		{
			var view0 = new TestView
			{
				Text = "Hello",
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			var view1 = new TestView
			{
				Text = "Hello",
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			SetUpTestLayout(StackOrientation.Vertical, view0, view1);

			var expectedHeight = TestAreaHeight / 2;

			Assert.AreEqual(expectedHeight, view0.Bounds.Height);
			Assert.AreEqual(expectedHeight, view1.Bounds.Height);

			Assert.AreEqual(0, view0.Bounds.Y);
			Assert.AreEqual(expectedHeight, view1.Bounds.Y);
		}

		[Test]
		public void TwoChildrenSplit5050Horizontal()
		{
			var view0 = new TestView
			{
				Text = "Hello",
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			var view1 = new TestView
			{
				Text = "Hello",
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			SetUpTestLayout(StackOrientation.Horizontal, view0, view1);

			var expectedWidth = TestAreaWidth / 2;

			Assert.AreEqual(expectedWidth, view0.Bounds.Width);
			Assert.AreEqual(expectedWidth, view1.Bounds.Width);

			Assert.AreEqual(0, view0.Bounds.X);
			Assert.AreEqual(expectedWidth, view1.Bounds.X);
		}

		static IEnumerable ExpansionYCases
		{
			get
			{
				yield return new object[] { LayoutOptions.StartAndExpand, 0 };
				yield return new object[] { LayoutOptions.EndAndExpand, (TestAreaHeight / 2) - TestViewHeight };
				yield return new object[] { LayoutOptions.CenterAndExpand, (TestAreaHeight / 4) - (TestViewHeight / 2) };
				yield return new object[] { LayoutOptions.FillAndExpand, 0 };
			}
		}

		static IEnumerable ExpansionXCases
		{
			get
			{
				yield return new object[] { LayoutOptions.StartAndExpand, 0 };
				yield return new object[] { LayoutOptions.EndAndExpand, (TestViewWidth / 2) - TestViewWidth };
				yield return new object[] { LayoutOptions.CenterAndExpand, (TestViewWidth / 4) - (TestViewWidth / 2) };
				yield return new object[] { LayoutOptions.FillAndExpand, 0 };
			}
		}

		[Test, TestCaseSource(nameof(ExpansionYCases))]
		public void AlignmentRespectedWithinVerticalSegment(LayoutOptions layoutOptions, double expectedY)
		{
			var view0 = new TestView
			{
				Text = "Hello",
				VerticalOptions = layoutOptions
			};

			var view1 = new TestView
			{
				Text = "Hello",
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			SetUpTestLayout(StackOrientation.Vertical, view0, view1);

			Assert.AreEqual(expectedY, view0.Bounds.Y);
		}

		[Test, TestCaseSource(nameof(ExpansionYCases))]
		public void AlignmentRespectedWithinHorizontalSegment(LayoutOptions layoutOptions, double expectedX)
		{
			var view0 = new TestView
			{
				Text = "Hello",
				HorizontalOptions = layoutOptions
			};

			var view1 = new TestView
			{
				Text = "Hello",
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			SetUpTestLayout(StackOrientation.Horizontal, view0, view1);

			Assert.AreEqual(expectedX, view0.Bounds.X);
		}

		[Test]
		public void StackLayoutWithNoExpansionDoesNotExpand()
		{
			var view0 = new TestView
			{
				Text = "Hello",
				VerticalOptions = LayoutOptions.Center
			};

			var view1 = new TestView
			{
				Text = "Hello",
				VerticalOptions = LayoutOptions.Start
			};

			SetUpTestLayout(StackOrientation.Vertical, view0, view1);

			Assert.AreEqual(0, view0.Bounds.X);
			Assert.AreEqual(0, view0.Bounds.Y);

			Assert.AreEqual(0, view1.Bounds.X);
			Assert.AreEqual(TestViewHeight, view1.Bounds.Y);
		}

		[Test]
		public void StackLayoutWithPointlessExpansionDoesNotExpand()
		{
			var view0 = new TestView
			{
				Text = "Hello",
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			var view1 = new TestView
			{
				Text = "Hello",
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			SetUpTestLayout(StackOrientation.Vertical, view0, view1);

			Assert.AreEqual(0, view0.Bounds.X);
			Assert.AreEqual(0, view0.Bounds.Y);

			Assert.AreEqual(0, view1.Bounds.X);
			Assert.AreEqual(TestViewHeight, view1.Bounds.Y);
		}

		[Test]
		public void UpdatedStackLayoutExpandsCorrectly()
		{
			var view0 = new TestView
			{
				Text = "Hello",
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			var view1 = new TestView
			{
				Text = "Hello",
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			var stackLayout = SetUpTestLayout(StackOrientation.Vertical, view0, view1);

			Assert.AreEqual(0, view0.Bounds.X);
			Assert.AreEqual(0, view0.Bounds.Y);

			Assert.AreEqual(0, view1.Bounds.X);
			Assert.AreEqual(TestAreaHeight / 2, view1.Bounds.Y);

			var view2 = new TestView
			{
				Text = "Hello",
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			stackLayout.Add(view2);

			(stackLayout as Maui.ILayout).CrossPlatformMeasure(TestAreaWidth, TestAreaHeight);
			(stackLayout as Maui.ILayout).CrossPlatformArrange(new Rect(0, 0, TestAreaWidth, TestAreaHeight));

			Assert.AreEqual(0, view0.Bounds.X);
			Assert.AreEqual(0, view0.Bounds.Y);

			Assert.AreEqual(0, view1.Bounds.X);
			Assert.AreEqual(TestAreaHeight / 3, view1.Bounds.Y);

			Assert.AreEqual(0, view2.Bounds.X);
			Assert.AreEqual(2 * (TestAreaHeight / 3), view2.Bounds.Y);
		}

		class ViewModel
		{
			public string Text { get; }

			public ViewModel(string text)
			{
				Text = text;
			}
		}

		[Test]
		public void AndExpandDoesNotInterfereWithBindingContext()
		{
			const string testText = "test text";

			var vm = new ViewModel(testText);

			var view0 = new TestView
			{
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			view0.SetBinding(Button.TextProperty, new Binding(nameof(ViewModel.Text)));

			var stackLayout = SetUpTestLayout(StackOrientation.Vertical, view0);
			stackLayout.BindingContext = vm;

			Assert.AreEqual(testText, view0.Text);
			Assert.AreEqual(vm, view0.BindingContext);

			MeasureAndArrange(stackLayout as Maui.ILayout);

			Assert.AreEqual(testText, view0.Text);
			Assert.AreEqual(vm, view0.BindingContext);
		}
	}
}
