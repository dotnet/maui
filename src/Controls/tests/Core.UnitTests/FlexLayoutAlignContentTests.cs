using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using FlexLayout = Microsoft.Maui.Controls.Compatibility.FlexLayout;


	public class FlexLayoutAlignContentTests : BaseTestFixture
	{
		[Fact]
		public void TestAlignContentFlexStart()
		{
			MockPlatformSizeService.Current.GetPlatformSizeFunc = (visual, width, height) => new SizeRequest(new Size(50, 10));

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				WidthRequest = 130,
				HeightRequest = 100,

				AlignContent = FlexAlignContent.Start,
				AlignItems = FlexAlignItems.Start,
				Direction = FlexDirection.Row,
				Wrap = FlexWrap.Wrap,
			};

			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view4);

			layout.Layout(new Rect(0, 0, 130, 100));

			Assert.Equal(layout.Bounds, new Rect(0, 0, 130, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 50, 10));
			Assert.Equal(view1.Bounds, new Rect(50, 0, 50, 10));
			Assert.Equal(view2.Bounds, new Rect(0, 10, 50, 10));
			Assert.Equal(view3.Bounds, new Rect(50, 10, 50, 10));
			Assert.Equal(view4.Bounds, new Rect(0, 20, 50, 10));
		}

		[Fact]
		public void TestAlignContentFlexStartWithoutHeightOnChildren()
		{
			MockPlatformSizeService.Current.GetPlatformSizeFunc = (visual, width, height) => new SizeRequest(new Size(50, 10));

			var layout = new FlexLayout
			{
				WidthRequest = 100,
				HeightRequest = 100,
				IsPlatformEnabled = true,

				AlignItems = FlexAlignItems.Start,
				Direction = FlexDirection.Column,
				Wrap = FlexWrap.Wrap,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view4);

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 50, 10));
			Assert.Equal(view1.Bounds, new Rect(0, 10, 50, 10));
			Assert.Equal(view2.Bounds, new Rect(0, 20, 50, 10));
			Assert.Equal(view3.Bounds, new Rect(0, 30, 50, 10));
			Assert.Equal(view4.Bounds, new Rect(0, 40, 50, 10));
		}

		[Fact]
		public void TestAlignContentFlexStartWithFlex()
		{
			MockPlatformSizeService.Current.GetPlatformSizeFunc = (visual, width, height) => new SizeRequest(new Size(0, 0));

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				WidthRequest = 100,
				HeightRequest = 120,

				Direction = FlexDirection.Column,
				Wrap = FlexWrap.Wrap,
				AlignItems = FlexAlignItems.Start,
			};

			var view0 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view0, 1);
			FlexLayout.SetBasis(view0, 0);
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view1, 1);
			FlexLayout.SetBasis(view1, 0);
			view1.WidthRequest = 50;
			view1.HeightRequest = 10;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view3, 1);
			FlexLayout.SetShrink(view3, 1);
			FlexLayout.SetBasis(view3, 0);
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rect(0, 0, 100, 120));

			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 120));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 50, 40));
			Assert.Equal(view1.Bounds, new Rect(0, 40, 50, 40));
			Assert.Equal(view2.Bounds, new Rect(0, 80, 50, 0));
			Assert.Equal(view3.Bounds, new Rect(0, 80, 50, 40));
			Assert.Equal(view4.Bounds, new Rect(0, 120, 50, 0));
		}

		[Fact]
		public void TestAlignContentFlexEnd()
		{
			MockPlatformSizeService.Current.GetPlatformSizeFunc = (visual, width, height) => new SizeRequest(new Size(50, 10));

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				WidthRequest = 100,
				HeightRequest = 100,

				Direction = FlexDirection.Column,
				AlignContent = FlexAlignContent.End,
				AlignItems = FlexAlignItems.Start,
				Wrap = FlexWrap.Wrap,
			};

			Func<View> createView = () => new View
			{
				IsPlatformEnabled = true,
				WidthRequest = 50,
				HeightRequest = 10,
			};
			var view0 = createView();
			layout.Children.Add(view0);

			var view1 = createView();
			layout.Children.Add(view1);

			var view2 = createView();
			layout.Children.Add(view2);

			var view3 = createView();
			layout.Children.Add(view3);

			var view4 = createView();
			layout.Children.Add(view4);

			var measure = layout.Measure(100, 100);
			layout.Layout(new Rect(0, 0, 100, 100));

			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(50, 0, 50, 10));
			Assert.Equal(view1.Bounds, new Rect(50, 10, 50, 10));
			Assert.Equal(view2.Bounds, new Rect(50, 20, 50, 10));
			Assert.Equal(view3.Bounds, new Rect(50, 30, 50, 10));
			Assert.Equal(view4.Bounds, new Rect(50, 40, 50, 10));

		}

		[Fact]
		public void TestAlignContentStretch()
		{
			MockPlatformSizeService.Current.GetPlatformSizeFunc = (visual, width, height) => new SizeRequest(new Size(0, 0));

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Column,
				AlignContent = FlexAlignContent.Stretch,
				AlignItems = FlexAlignItems.Start,
				Wrap = FlexWrap.Wrap,
				WidthRequest = 150,
				HeightRequest = 100
			};
			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rect(0, 0, 150, 100));

			Assert.Equal(0f, layout.X);
			Assert.Equal(0f, layout.Y);
			Assert.Equal(150f, layout.Width);
			Assert.Equal(100f, layout.Height);

			Assert.Equal(0f, view0.X);
			Assert.Equal(0f, view0.Y);
			Assert.Equal(50f, view0.Width);
			Assert.Equal(0f, view0.Height);

			Assert.Equal(0f, view1.X);
			Assert.Equal(0f, view1.Y);
			Assert.Equal(50f, view1.Width);
			Assert.Equal(0f, view1.Height);

			Assert.Equal(0f, view2.X);
			Assert.Equal(0f, view2.Y);
			Assert.Equal(50f, view2.Width);
			Assert.Equal(0f, view2.Height);

			Assert.Equal(0f, view3.X);
			Assert.Equal(0f, view3.Y);
			Assert.Equal(50f, view3.Width);
			Assert.Equal(0f, view3.Height);

			Assert.Equal(0f, view4.X);
			Assert.Equal(0f, view4.Y);
			Assert.Equal(50f, view4.Width);
			Assert.Equal(0f, view4.Height);
		}

		[Fact]
		public void TestAlignContentSpaceBetween()
		{
			MockPlatformSizeService.Current.GetPlatformSizeFunc = (visual, width, height) => new SizeRequest(new Size(50, 10));

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				WidthRequest = 130,
				HeightRequest = 100,

				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.SpaceBetween,
				Wrap = FlexWrap.Wrap,
			};

			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10 };
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10 };
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10 };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10 };
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10 };
			layout.Children.Add(view4);

			layout.Layout(new Rect(0, 0, 130, 100));

			Assert.Equal(layout.Bounds, new Rect(0, 0, 130, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 50, 10));
			Assert.Equal(view1.Bounds, new Rect(50, 0, 50, 10));
			Assert.Equal(view2.Bounds, new Rect(0, 45, 50, 10));
			Assert.Equal(view3.Bounds, new Rect(50, 45, 50, 10));
			Assert.Equal(view4.Bounds, new Rect(0, 90, 50, 10));
		}

		[Fact]
		public void TestAlignContentSpaceAround()
		{
			MockPlatformSizeService.Current.GetPlatformSizeFunc = (visual, width, height) => new SizeRequest(new Size(50, 10));

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				WidthRequest = 140,
				HeightRequest = 120,

				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.SpaceAround,
				Wrap = FlexWrap.Wrap,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view4);

			layout.Layout(new Rect(0, 0, 140, 120));

			Assert.Equal(layout.Bounds, new Rect(0, 0, 140, 120));
			Assert.Equal(view0.Bounds, new Rect(0, 15, 50, 10));
			Assert.Equal(view1.Bounds, new Rect(50, 15, 50, 10));
			Assert.Equal(view2.Bounds, new Rect(0, 55, 50, 10));
			Assert.Equal(view3.Bounds, new Rect(50, 55, 50, 10));
			Assert.Equal(view4.Bounds, new Rect(0, 95, 50, 10));
		}

		[Fact]
		public void TestAlignContentStretchRow()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.Stretch,
				AlignItems = FlexAlignItems.Start,
				Wrap = FlexWrap.Wrap,
				WidthRequest = 150,
				HeightRequest = 100
			};
			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rect(0, 0, 150, 100));

			Assert.Equal(0f, layout.X);
			Assert.Equal(0f, layout.Y);
			Assert.Equal(150f, layout.Width);
			Assert.Equal(100f, layout.Height);

			Assert.Equal(0f, view0.X);
			Assert.Equal(0f, view0.Y);
			Assert.Equal(50f, view0.Width);
			Assert.Equal(20f, view0.Height);

			Assert.Equal(50f, view1.X);
			Assert.Equal(0f, view1.Y);
			Assert.Equal(50f, view1.Width);
			Assert.Equal(20f, view1.Height);

			Assert.Equal(100f, view2.X);
			Assert.Equal(0f, view2.Y);
			Assert.Equal(50f, view2.Width);
			Assert.Equal(20f, view2.Height);

			Assert.Equal(0f, view3.X);
			Assert.Equal(50f, view3.Y);
			Assert.Equal(50f, view3.Width);
			Assert.Equal(20f, view3.Height);

			Assert.Equal(50f, view4.X);
			Assert.Equal(50f, view4.Y);
			Assert.Equal(50f, view4.Width);
			Assert.Equal(20f, view4.Height);
		}

		[Fact]
		public void TestAlignContentStretchRowWithChildren()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				WidthRequest = 150,
				HeightRequest = 100,

				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.Stretch,
				Wrap = FlexWrap.Wrap,
			};
			var view0 = new FlexLayout { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view0);

			//var view0_child0 = new View { IsPlatformEnabled = true };
			//FlexLayout.SetGrow(view0_child0, 1);
			//FlexLayout.SetShrink(view0_child0, 1);
			//FlexLayout.SetBasis(view0_child0, 0);
			//view0.Children.Add(view0_child0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view4);

			layout.Layout(new Rect(0, 0, 150, 100));

			Assert.Equal(layout.Bounds, new Rect(0, 0, 150, 100));

			Assert.Equal(0f, view0.X);
			Assert.Equal(0f, view0.Y);
			Assert.Equal(50f, view0.Width);
			Assert.Equal(50f, view0.Height);

			//Assert.Equal(0f, view0_child0.X);
			//Assert.Equal(0f, view0_child0.Y);
			//Assert.Equal(50f, view0_child0.Width);
			//Assert.Equal(50f, view0_child0.Height);

			Assert.Equal(50f, view1.X);
			Assert.Equal(0f, view1.Y);
			Assert.Equal(50f, view1.Width);
			Assert.Equal(50f, view1.Height);

			Assert.Equal(100f, view2.X);
			Assert.Equal(0f, view2.Y);
			Assert.Equal(50f, view2.Width);
			Assert.Equal(50f, view2.Height);

			Assert.Equal(0f, view3.X);
			Assert.Equal(50f, view3.Y);
			Assert.Equal(50f, view3.Width);
			Assert.Equal(50f, view3.Height);

			Assert.Equal(50f, view4.X);
			Assert.Equal(50f, view4.Y);
			Assert.Equal(50f, view4.Width);
			Assert.Equal(50f, view4.Height);
		}

		[Fact]
		public void TestAlignContentStretchRowWithFlex()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				AlignContent = FlexAlignContent.Stretch,
				Direction = FlexDirection.Row,
				Wrap = FlexWrap.Wrap,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			FlexLayout.SetGrow(view1, 1);
			FlexLayout.SetShrink(view1, 1);
			FlexLayout.SetBasis(view1, 0);
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			FlexLayout.SetGrow(view3, 1);
			FlexLayout.SetShrink(view3, 1);
			FlexLayout.SetBasis(view3, 0);
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			layout.Children.Add(view4);

			layout.Layout(new Rect(0, 0, 150, 100));

			Assert.Equal(layout.Bounds, new Rect(0, 0, 150, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 50, 100));
			Assert.Equal(view1.Bounds, new Rect(50, 0, 0, 100));
			Assert.Equal(view2.Bounds, new Rect(50, 0, 50, 100));
			Assert.Equal(view3.Bounds, new Rect(100, 0, 0, 100));
			Assert.Equal(view4.Bounds, new Rect(100, 0, 50, 100));
		}

		[Fact]
		public void TestAlignContentStretchRowWithFlexNoShrink()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.Stretch,
				Wrap = FlexWrap.Wrap,
				WidthRequest = 150,
				HeightRequest = 100
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			FlexLayout.SetGrow(view1, 1);
			FlexLayout.SetShrink(view1, 1);
			FlexLayout.SetBasis(view1, 0);
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			FlexLayout.SetGrow(view3, 1);
			FlexLayout.SetBasis(view3, 0);
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			layout.Children.Add(view4);

			layout.Layout(new Rect(0, 0, 150, 100));

			Assert.Equal(0f, layout.X);
			Assert.Equal(0f, layout.Y);
			Assert.Equal(150f, layout.Width);
			Assert.Equal(100f, layout.Height);

			Assert.Equal(0f, view0.X);
			Assert.Equal(0f, view0.Y);
			Assert.Equal(50f, view0.Width);
			Assert.Equal(100f, view0.Height);

			Assert.Equal(50f, view1.X);
			Assert.Equal(0f, view1.Y);
			Assert.Equal(0f, view1.Width);
			Assert.Equal(100f, view1.Height);

			Assert.Equal(50f, view2.X);
			Assert.Equal(0f, view2.Y);
			Assert.Equal(50f, view2.Width);
			Assert.Equal(100f, view2.Height);

			Assert.Equal(100f, view3.X);
			Assert.Equal(0f, view3.Y);
			Assert.Equal(0f, view3.Width);
			Assert.Equal(100f, view3.Height);

			Assert.Equal(100f, view4.X);
			Assert.Equal(0f, view4.Y);
			Assert.Equal(50f, view4.Width);
			Assert.Equal(100f, view4.Height);
		}

		[Fact(Skip = "")]
		public void TestAlignContentStretchRowWithMargin()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				WidthRequest = 150,
				HeightRequest = 100,

				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.Stretch,
				Wrap = FlexWrap.Wrap,
			};
			var view0 = new View
			{
				IsPlatformEnabled = true,
				WidthRequest = 50,
				HeightRequest = 20,
			};

			layout.Children.Add(view0);

			var view1 = new View
			{
				IsPlatformEnabled = true,
				Margin = 10,
				WidthRequest = 50,
				HeightRequest = 20,
			};
			layout.Children.Add(view1);

			var view2 = new View
			{
				IsPlatformEnabled = true,
				WidthRequest = 50,
				HeightRequest = 20,
			};
			layout.Children.Add(view2);

			var view3 = new View
			{
				IsPlatformEnabled = true,
				Margin = new Thickness(10),
				WidthRequest = 50,
				HeightRequest = 20,
			};
			layout.Children.Add(view3);

			var view4 = new View
			{
				IsPlatformEnabled = true,
				WidthRequest = 50,
				HeightRequest = 20,
			};
			layout.Children.Add(view4);

			layout.Layout(new Rect(0, 0, 150, 100));

			Assert.Equal(layout.Bounds, new Rect(0, 0, 150, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 50, 20));
			Assert.Equal(view1.Bounds, new Rect(60, 10, 50, 20));
			Assert.Equal(view2.Bounds, new Rect(0, 40, 50, 20));
			Assert.Equal(view3.Bounds, new Rect(60, 50, 50, 20));
			Assert.Equal(view4.Bounds, new Rect(0, 80, 50, 20));
		}

		[Fact]
		public void TestAlignContentStretchRowWithSingleRow()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.Stretch,
				Wrap = FlexWrap.Wrap,
				WidthRequest = 150,
				HeightRequest = 100
			};
			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			layout.Layout(new Rect(0, 0, 150, 100));

			Assert.Equal(0f, layout.X);
			Assert.Equal(0f, layout.Y);
			Assert.Equal(150f, layout.Width);
			Assert.Equal(100f, layout.Height);

			Assert.Equal(0f, view0.X);
			Assert.Equal(0f, view0.Y);
			Assert.Equal(50f, view0.Width);
			Assert.Equal(100f, view0.Height);

			Assert.Equal(50f, view1.X);
			Assert.Equal(0f, view1.Y);
			Assert.Equal(50f, view1.Width);
			Assert.Equal(100f, view1.Height);
		}

		[Fact]
		public void TestAlignContentStretchRowWithFixedHeight()
		{
			MockPlatformSizeService.Current.GetPlatformSizeFunc = (visual, width, height) => new SizeRequest(new Size(0, 0));

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.Stretch,
				AlignItems = FlexAlignItems.Start,
				Wrap = FlexWrap.Wrap,
				WidthRequest = 150,
				HeightRequest = 100
			};
			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			view1.HeightRequest = 60;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rect(0, 0, 150, 100));

			Assert.Equal(0f, layout.X);
			Assert.Equal(0f, layout.Y);
			Assert.Equal(150f, layout.Width);
			Assert.Equal(100f, layout.Height);

			Assert.Equal(0f, view0.X);
			Assert.Equal(0f, view0.Y);
			Assert.Equal(50f, view0.Width);
			Assert.Equal(0f, view0.Height);

			Assert.Equal(50f, view1.X);
			Assert.Equal(0f, view1.Y);
			Assert.Equal(50f, view1.Width);
			Assert.Equal(60f, view1.Height);

			Assert.Equal(100f, view2.X);
			Assert.Equal(0f, view2.Y);
			Assert.Equal(50f, view2.Width);
			Assert.Equal(0f, view2.Height);

			Assert.Equal(0f, view3.X);
			Assert.Equal(80f, view3.Y);
			Assert.Equal(50f, view3.Width);
			Assert.Equal(0f, view3.Height);

			Assert.Equal(50f, view4.X);
			Assert.Equal(80f, view4.Y);
			Assert.Equal(50f, view4.Width);
			Assert.Equal(0f, view4.Height);
		}
	}
}
