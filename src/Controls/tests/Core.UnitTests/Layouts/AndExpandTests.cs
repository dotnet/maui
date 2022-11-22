#nullable enable
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[Category("Layout")]
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

		[Fact]
		public void SingleChildExpandsToFillVertical()
		{
			var view0 = new TestView
			{
				Text = "Hello",
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			var layout = SetUpTestLayout(StackOrientation.Vertical, view0);

			Assert.Equal(TestAreaWidth, view0.Bounds.Width);
			Assert.Equal(TestAreaHeight, view0.Bounds.Height);
			Assert.Equal(0, view0.Bounds.X);
			Assert.Equal(0, view0.Bounds.Y);
		}

		[Fact]
		public void SingleChildExpandsToFillHorizontal()
		{
			var view0 = new TestView
			{
				Text = "Hello",
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			var layout = SetUpTestLayout(StackOrientation.Horizontal, view0);

			Assert.Equal(TestAreaWidth, view0.Bounds.Width);
			Assert.Equal(TestAreaHeight, view0.Bounds.Height);
			Assert.Equal(0, view0.Bounds.X);
			Assert.Equal(0, view0.Bounds.Y);
		}

		[Fact]
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

			Assert.Equal(expectedHeight, view0.Bounds.Height);
			Assert.Equal(expectedHeight, view1.Bounds.Height);

			Assert.Equal(0, view0.Bounds.Y);
			Assert.Equal(expectedHeight, view1.Bounds.Y);
		}

		[Fact]
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

			Assert.Equal(expectedWidth, view0.Bounds.Width);
			Assert.Equal(expectedWidth, view1.Bounds.Width);

			Assert.Equal(0, view0.Bounds.X);
			Assert.Equal(expectedWidth, view1.Bounds.X);
		}

		static IEnumerable<object[]> ExpansionYCases()
		{
			yield return new object[] { LayoutOptions.StartAndExpand, 0 };
			yield return new object[] { LayoutOptions.EndAndExpand, (TestAreaHeight / 2) - TestViewHeight };
			yield return new object[] { LayoutOptions.CenterAndExpand, (TestAreaHeight / 4) - (TestViewHeight / 2) };
			yield return new object[] { LayoutOptions.FillAndExpand, 0 };
		}

		static IEnumerable<object[]> ExpansionXCases()
		{
			yield return new object[] { LayoutOptions.StartAndExpand, 0 };
			yield return new object[] { LayoutOptions.EndAndExpand, (TestAreaWidth / 2) - TestViewWidth };
			yield return new object[] { LayoutOptions.CenterAndExpand, (TestAreaWidth / 4) - (TestViewWidth / 2) };
			yield return new object[] { LayoutOptions.FillAndExpand, 0 };
		}

		[Theory, MemberData(nameof(ExpansionYCases))]
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

			Assert.Equal(expectedY, view0.Bounds.Y);
		}

		[Theory, MemberData(nameof(ExpansionXCases))]
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

			Assert.Equal(expectedX, view0.Bounds.X);
		}

		[Fact]
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

			Assert.Equal(0, view0.Bounds.X);
			Assert.Equal(0, view0.Bounds.Y);

			Assert.Equal(0, view1.Bounds.X);
			Assert.Equal(TestViewHeight, view1.Bounds.Y);
		}

		[Fact]
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

			Assert.Equal(0, view0.Bounds.X);
			Assert.Equal(0, view0.Bounds.Y);

			Assert.Equal(0, view1.Bounds.X);
			Assert.Equal(TestViewHeight, view1.Bounds.Y);
		}

		[Fact]
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

			Assert.Equal(0, view0.Bounds.X);
			Assert.Equal(0, view0.Bounds.Y);

			Assert.Equal(0, view1.Bounds.X);
			Assert.Equal(TestAreaHeight / 2, view1.Bounds.Y);

			var view2 = new TestView
			{
				Text = "Hello",
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			stackLayout.Add(view2);

			(stackLayout as Maui.ILayout).CrossPlatformMeasure(TestAreaWidth, TestAreaHeight);
			(stackLayout as Maui.ILayout).CrossPlatformArrange(new Rect(0, 0, TestAreaWidth, TestAreaHeight));

			Assert.Equal(0, view0.Bounds.X);
			Assert.Equal(0, view0.Bounds.Y);

			Assert.Equal(0, view1.Bounds.X);
			Assert.Equal(TestAreaHeight / 3, view1.Bounds.Y);

			Assert.Equal(0, view2.Bounds.X);
			Assert.Equal(2 * (TestAreaHeight / 3), view2.Bounds.Y);
		}

		class ViewModel
		{
			public string Text { get; }

			public ViewModel(string text)
			{
				Text = text;
			}
		}

		[Fact]
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

			Assert.Equal(testText, view0.Text);
			Assert.Equal(vm, view0.BindingContext);

			MeasureAndArrange(stackLayout as Maui.ILayout);

			Assert.Equal(testText, view0.Text);
			Assert.Equal(vm, view0.BindingContext);
		}

		[Fact]
		public void EnsuresMeasure()
		{
			var view0 = new TestView
			{
				Text = "Hello",
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			var layout = new StackLayout();
			layout.Add(view0);

			(layout as Maui.ILayout).CrossPlatformArrange(new Rect(0, 0, 100, 100));
		}
	}
}
