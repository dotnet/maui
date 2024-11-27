using System;
using System.Globalization;
using System.Threading;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using FlexLayout = Microsoft.Maui.Controls.Compatibility.FlexLayout;


	public class FlexLayoutTests : BaseTestFixture
	{
		[Fact]
		public void TestBasicLayout()
		{
			var label1 = MockPlatformSizeService.Sub<Label>();
			var label2 = MockPlatformSizeService.Sub<Label>();

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					label1,
					label2,
				}
			};

			layout.Layout(new Rect(0, 0, 912, 912));

			Assert.Equal(layout.Bounds, new Rect(0, 0, 912, 912));
			Assert.Equal(label1.Bounds, new Rect(0, 0, 100, 912));
			Assert.Equal(label2.Bounds, new Rect(100, 0, 100, 912));
		}

		[Fact]
		public void TestBasicLayoutWithElementsWidth()
		{
			var label1 = MockPlatformSizeService.Sub<Label>(width: 120);
			var label2 = MockPlatformSizeService.Sub<Label>(width: 120);

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					label1,
					label2,
				}
			};

			layout.Layout(new Rect(0, 0, 912, 912));

			Assert.Equal(layout.Bounds, new Rect(0, 0, 912, 912));
			Assert.Equal(label1.Bounds, new Rect(0, 0, 120, 912));
			Assert.Equal(label2.Bounds, new Rect(120, 0, 120, 912));

		}

		[Fact]
		public void TestBasicLayoutWithElementsWidthAndMargin()
		{
			var label1 = MockPlatformSizeService.Sub<Label>(width: 100, margin: new(5, 0, 0, 0));
			var label2 = MockPlatformSizeService.Sub<Label>(width: 100, margin: new(5, 0, 0, 0));

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					label1,
					label2,
				}
			};

			layout.Layout(new Rect(0, 0, 912, 912));

			Assert.Equal(912, layout.Width);
			Assert.Equal(912, layout.Height);

			Assert.Equal(new Rect(5, 0, 100, 912), label1.Bounds);
			Assert.Equal(new Rect(110, 0, 100, 912), label2.Bounds);
		}

		[Fact]
		public void TestSetBounds()
		{
			var layoutSize = new Size(320, 50);

			var layout = new FlexLayout
			{
				Direction = FlexDirection.Row,
				AlignItems = FlexAlignItems.Start,
				IsPlatformEnabled = true
			};

			var label1 = MockPlatformSizeService.Sub<Label>();
			FlexLayout.SetGrow(label1, 1);
			layout.Children.Add(label1);

			var label2 = MockPlatformSizeService.Sub<Label>();
			FlexLayout.SetGrow(label2, 1);
			layout.Children.Add(label2);

			var label3 = MockPlatformSizeService.Sub<Label>();
			FlexLayout.SetGrow(label3, 1);
			layout.Children.Add(label3);

			layout.Layout(new Rect(0, 0, layoutSize.Width, layoutSize.Height));

			Assert.Equal(label2.Bounds.Left, Math.Max(label1.Bounds.Left, label1.Bounds.Right), 1);
			Assert.Equal(label3.Bounds.Left, Math.Max(label2.Bounds.Left, label2.Bounds.Right), 1);

			double totalWidth = 0;
			foreach (var view in layout.Children)
			{
				totalWidth += view.Frame.Width;
			}

			Assert.Equal(layoutSize.Width, totalWidth, 2);
		}

		[Fact]
		public void TestRelayoutOnChildrenRemoved()
		{
			var layoutSize = new Size(300, 50);

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row
			};

			var label1 = MockPlatformSizeService.Sub<Label>();
			FlexLayout.SetGrow(label1, 1);
			layout.Children.Add(label1);

			var label2 = MockPlatformSizeService.Sub<Label>();
			FlexLayout.SetGrow(label2, 1);
			layout.Children.Add(label2);

			var label3 = MockPlatformSizeService.Sub<Label>();
			FlexLayout.SetGrow(label3, 1);
			layout.Children.Add(label3);

			layout.Layout(new Rect(0, 0, layoutSize.Width, layoutSize.Height));

			foreach (var view in layout.Children)
				Assert.Equal(100, view.Frame.Width);

			layout.Children.Remove(label3);

			Assert.Equal(150, label1.Bounds.Width);
			Assert.Equal(150, label2.Bounds.Width);
			Assert.Equal(100, label3.Bounds.Width);
		}

		[Fact]
		public void TestFlexLayoutIsIncludeChangeWorksOnSecondPass()
		{
			var layoutSize = new Size(300, 50);
			var layout = new FlexLayout
			{
				Direction = FlexDirection.Row,
				IsPlatformEnabled = true,
			};

			var label1 = MockPlatformSizeService.Sub<Label>();
			FlexLayout.SetGrow(label1, 1);
			layout.Children.Add(label1);

			var label2 = MockPlatformSizeService.Sub<Label>();
			FlexLayout.SetGrow(label2, 1);
			layout.Children.Add(label2);

			var label3 = MockPlatformSizeService.Sub<Label>();
			FlexLayout.SetGrow(label3, 1);

			layout.Layout(new Rect(0, 0, layoutSize.Width, layoutSize.Height));

			Assert.Equal(150, label1.Bounds.Width);
			Assert.Equal(150, label2.Bounds.Width);
			Assert.Equal(-1, label3.Bounds.Width);

			layout.Children.Add(label3);

			layout.Layout(new Rect(0, 0, layoutSize.Width, layoutSize.Height));
			Assert.Equal(100, label1.Bounds.Width);
			Assert.Equal(100, label2.Bounds.Width);
			Assert.Equal(100, label3.Bounds.Width);
		}

		[Fact]
		// fixed at https://github.com/xamarin/flex/commit/0ccb9f1625abdc5400def29651373937bf6610cd
		public void TestSwapChildrenOrder()
		{
			var layoutSize = new Size(300, 50);

			var layout = new FlexLayout
			{
				Direction = FlexDirection.Row,
				IsPlatformEnabled = true,
			};

			var label0 = MockPlatformSizeService.Sub<Label>(text: "Label0");
			FlexLayout.SetGrow(label0, 1);
			layout.Children.Add(label0);

			var label1 = MockPlatformSizeService.Sub<Label>(text: "Label1");
			FlexLayout.SetGrow(label1, 1);
			layout.Children.Add(label1);

			var label2 = MockPlatformSizeService.Sub<Label>(text: "Label2");
			FlexLayout.SetGrow(label2, 1);
			layout.Children.Add(label2);

			layout.Layout(new Rect(0, 0, layoutSize.Width, layoutSize.Height));

			Assert.Equal(new Rect(0, 0, 100, 50), label0.Bounds);
			Assert.Equal(new Rect(100, 0, 100, 50), label1.Bounds);
			Assert.Equal(new Rect(200, 0, 100, 50), label2.Bounds);

			var lastItem = layout.Children[2];
			Assert.Same(lastItem, label2);

			layout.Children.Remove(lastItem);
			Assert.Equal(new Rect(0, 0, 150, 50), label0.Bounds);
			Assert.Equal(new Rect(150, 0, 150, 50), label1.Bounds);

			layout.Children.Insert(0, lastItem);

			Assert.Equal(new Rect(0, 0, 100, 50), label2.Bounds);
			Assert.Equal(new Rect(100, 0, 100, 50), label0.Bounds);
			Assert.Equal(new Rect(200, 0, 100, 50), label1.Bounds);
		}

		[Fact]
		public void TestSizeThatFits()
		{
			var layout = new FlexLayout
			{
				Direction = FlexDirection.Row,
				AlignItems = FlexAlignItems.Start,
				IsPlatformEnabled = true
			};

			var label1 = MockPlatformSizeService.Sub<Label>(
				useRealisticLabelMeasure: true,
				lineBreak: LineBreakMode.TailTruncation,
				text: @"This is a very very very very very very very very long piece of text.");
			FlexLayout.SetShrink(label1, 1);
			layout.Children.Add(label1);

			var label2 = MockPlatformSizeService.Sub<Label>(
				useRealisticLabelMeasure: true,
				text: "",
				width: 10,
				height: 10);
			layout.Children.Add(label2);
			layout.Layout(new Rect(0, 0, 320, 50));

			var label2Size = label2.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
			Assert.Equal(10, label2Size.Request.Height);
			Assert.Equal(10, label2Size.Request.Width);

			var label1Size = label1.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
			//	var layoutSize = layout.Measure(-1, -1);
		}

		[Fact]
		public void TestNesting()
		{
			var header = MockPlatformSizeService.Sub<View>(height: 50);
			var footer = MockPlatformSizeService.Sub<View>(height: 50);
			Func<View> createItem = () =>
			{
				var v = MockPlatformSizeService.Sub<View>(width: 50, margin: 5);
				FlexLayout.SetGrow(v, 1);
				return v;
			};

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					header,
					new FlexLayout{
						Direction = FlexDirection.Row,
						IsPlatformEnabled = true,
						Children = {
							createItem(),
							createItem(),
							createItem(),
							createItem(),
							createItem(),
						}
					},
					footer,
				},
				Direction = FlexDirection.Column,
			};

			var inner = layout.Children[1] as FlexLayout;
			FlexLayout.SetGrow(inner, 1);

			layout.Layout(new Rect(0, 0, 300, 600));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 300, 600));
			Assert.Equal(header.Bounds, new Rect(0, 0, 300, 50));
			Assert.Equal(inner.Bounds, new Rect(0, 50, 300, 500));
			Assert.Equal(inner.Children[0].Frame, new Rect(5, 5, 50, 490));
			Assert.Equal(inner.Children[1].Frame, new Rect(65, 5, 50, 490));
			Assert.Equal(inner.Children[2].Frame, new Rect(125, 5, 50, 490));
			Assert.Equal(inner.Children[3].Frame, new Rect(185, 5, 50, 490));
			Assert.Equal(inner.Children[4].Frame, new Rect(245, 5, 50, 490));

			Assert.Equal(footer.Bounds, new Rect(0, 550, 300, 50));
		}

		[Fact]
		public void TestMeasuring()
		{
			var label = MockPlatformSizeService.Sub<Label>();
			var Layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row,
				Wrap = FlexWrap.Wrap,
				Children = {
					label,
				}
			};

			//measure sith +inf as main-axis
			var measure = Layout.Measure(double.PositiveInfinity, 40, MeasureFlags.None);
			Assert.Equal(measure.Request, new Size(100, 40));

			//measure sith +inf as cross-axis
			measure = Layout.Measure(200, double.PositiveInfinity, MeasureFlags.None);
			Assert.Equal(measure.Request, new Size(200, 20));

			//measure with +inf as both axis
			measure = Layout.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
			Assert.Equal(measure.Request, new Size(100, 20));

		}

		[Fact]
		public void TestMarginsWithWrap()
		{
			var label0 = MockPlatformSizeService.Sub<Label>(margin: 6);
			var label1 = MockPlatformSizeService.Sub<Label>(margin: 6);
			var label2 = MockPlatformSizeService.Sub<Label>();
			FlexLayout.SetGrow(label0, 0);
			FlexLayout.SetBasis(label0, new FlexBasis(.5f, true));
			FlexLayout.SetGrow(label1, 0);
			FlexLayout.SetBasis(label1, new FlexBasis(.5f, true));
			FlexLayout.SetGrow(label2, 0);
			FlexLayout.SetBasis(label2, new FlexBasis(1f, true));
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row,
				Wrap = FlexWrap.Wrap,
				AlignItems = FlexAlignItems.Start,
				AlignContent = FlexAlignContent.Start,
				Children = {
					label0,
					label1,
					label2,
				}
			};

			var measure = layout.Measure(300, double.PositiveInfinity, MeasureFlags.None);
			Assert.Equal(measure.Request, new Size(300, 52));
			layout.Layout(new Rect(0, 0, 300, 300));
			Assert.Equal(label0.Bounds, new Rect(6, 6, 138, 20));
			Assert.Equal(label1.Bounds, new Rect(156, 6, 138, 20));
			Assert.Equal(label2.Bounds, new Rect(0, 32, 300, 20));
		}

		[Fact]
		//https://github.com/xamarin/Microsoft.Maui.Controls/issues/2551
		public void TestReverseWithGrow()
		{
			var label0 = MockPlatformSizeService.Sub<Label>();
			FlexLayout.SetGrow(label0, 1);
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.ColumnReverse,
				Children = {
					label0,
				}
			};

			layout.Layout(new Rect(0, 0, 300, 300));
			Assert.Equal(label0.Bounds, new Rect(0, 0, 300, 300));
		}

		[Fact]
		public void TestIsVisible()
		//https://github.com/xamarin/Microsoft.Maui.Controls/issues/2593
		{
			var label0 = MockPlatformSizeService.Sub<Label>();
			var label1 = MockPlatformSizeService.Sub<Label>();
			var label2 = MockPlatformSizeService.Sub<Label>();
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Column,
				Children = {
					label0,
					label1,
					label2,
				}
			};

			var handler = Substitute.For<IViewHandler>();
			layout.Handler = handler;

			layout.Layout(new Rect(0, 0, 300, 300));
			Assert.Equal(label0.Bounds, new Rect(0, 0, 300, 20));
			Assert.Equal(label1.Bounds, new Rect(0, 20, 300, 20));
			Assert.Equal(label2.Bounds, new Rect(0, 40, 300, 20));

			label1.IsVisible = false;

			// Changing the visibility of the label should have triggered a measure invalidation in the layout
			AssertInvalidated(handler);

			// Fake a native invalidation
			layout.ForceLayout();

			Assert.Equal(label0.Bounds, new Rect(0, 0, 300, 20));
			Assert.Equal(label2.Bounds, new Rect(0, 20, 300, 20));

			label0.IsVisible = false;
			label1.IsVisible = true;

			// Verify the visibility changes invalidated the layout
			AssertInvalidated(handler);

			// Fake a native invalidation
			layout.ForceLayout();

			Assert.Equal(label1.Bounds, new Rect(0, 0, 300, 20));
			Assert.Equal(label2.Bounds, new Rect(0, 20, 300, 20));
		}

		[Fact]
		public void ChangingGrowTriggersLayout()
		//https://github.com/xamarin/Microsoft.Maui.Controls/issues/2821
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Column,
			};

			var handler = Substitute.For<IViewHandler>();
			layout.Handler = handler;

			layout.Layout(new Rect(0, 0, 300, 300));
			for (var i = 0; i < 3; i++)
			{
				var box = new BoxView
				{
					IsPlatformEnabled = true,
				};
				layout.Children.Add(box);
				FlexLayout.SetGrow(box, 1f);
			}

			// Verify the changes invalidated the layout
			AssertInvalidated(handler);

			// Fake a native invalidation
			layout.ForceLayout();

			Assert.Equal(layout.Children[2].Frame, new Rect(0, 200, 300, 100));
		}

		[Fact]
		public void PaddingOnLayout()
		//https://github.com/xamarin/Microsoft.Maui.Controls/issues/2663
		{
			var label0 = MockPlatformSizeService.Sub<Label>();
			var label1 = MockPlatformSizeService.Sub<Label>();
			var label2 = MockPlatformSizeService.Sub<Label>();
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				JustifyContent = FlexJustify.SpaceBetween,
				AlignItems = FlexAlignItems.Start,

				Padding = new Thickness(20, 10, 20, 0),
				Children = {
					label0,
					label1,
					label2,
				}
			};

			layout.Layout(new Rect(0, 0, 500, 300));
			Assert.Equal(layout.Children[0].Frame, new Rect(20, 10, 100, 20));
			Assert.Equal(layout.Children[2].Frame, new Rect(380, 10, 100, 20));
		}

		void AssertInvalidated(IViewHandler handler)
		{
			handler.Received().Invoke(Arg.Is(nameof(IView.InvalidateMeasure)), Arg.Any<object>());
			handler.ClearReceivedCalls();
		}
	}
}
