using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using FlexLayout = Microsoft.Maui.Controls.Compatibility.FlexLayout;


	public class FlexLayoutFlexDirectionTests : BaseTestFixture
	{
		[Fact]
		public void TestFlexDirectionColumnWithoutHeight()
		{
			var view0 = new View { IsPlatformEnabled = true, HeightRequest = 10 };
			var view1 = new View { IsPlatformEnabled = true, HeightRequest = 10 };
			var view2 = new View { IsPlatformEnabled = true, HeightRequest = 10 };
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
					view1,
					view2,
				},

				Direction = FlexDirection.Column,
			};

			var sizeRequest = layout.Measure(100, double.PositiveInfinity, MeasureFlags.None);
			layout.Layout(new Rect(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 30));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 100, 10));
			Assert.Equal(view1.Bounds, new Rect(0, 10, 100, 10));
			Assert.Equal(view2.Bounds, new Rect(0, 20, 100, 10));
		}

		[Fact]
		public void TestFlexDirectionRowNoWidth()
		{
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, };
			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 10, };
			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 10, };
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
					view1,
					view2,
				},

				Direction = FlexDirection.Row,
			};


			var measure = layout.Measure(double.PositiveInfinity, 100, MeasureFlags.None);
			layout.Layout(new Rect(0, 0, measure.Request.Width, measure.Request.Height));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 30, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 10, 100));
			Assert.Equal(view1.Bounds, new Rect(10, 0, 10, 100));
			Assert.Equal(view2.Bounds, new Rect(20, 0, 10, 100));
		}

		[Fact]
		public void TestFlexDirectionColumn()
		{
			var view0 = new View { IsPlatformEnabled = true, HeightRequest = 10 };
			var view1 = new View { IsPlatformEnabled = true, HeightRequest = 10 };
			var view2 = new View { IsPlatformEnabled = true, HeightRequest = 10 };
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
					view1,
					view2,
				},

				Direction = FlexDirection.Column,
			};

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 100, 10));
			Assert.Equal(view1.Bounds, new Rect(0, 10, 100, 10));
			Assert.Equal(view2.Bounds, new Rect(0, 20, 100, 10));
		}

		[Fact]
		public void TestFlexDirectionRow()
		{
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, };
			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 10, };
			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 10, };
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
					view1,
					view2,
				},

				Direction = FlexDirection.Row,
			};

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 10, 100));
			Assert.Equal(view1.Bounds, new Rect(10, 0, 10, 100));
			Assert.Equal(view2.Bounds, new Rect(20, 0, 10, 100));
		}

		[Fact]
		public void TestFlexDirectionColumnReverse()
		{
			var view0 = new View { IsPlatformEnabled = true, HeightRequest = 10 };
			var view1 = new View { IsPlatformEnabled = true, HeightRequest = 10 };
			var view2 = new View { IsPlatformEnabled = true, HeightRequest = 10 };
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
					view1,
					view2,
				},

				Direction = FlexDirection.ColumnReverse,
			};

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 90, 100, 10));
			Assert.Equal(view1.Bounds, new Rect(0, 80, 100, 10));
			Assert.Equal(view2.Bounds, new Rect(0, 70, 100, 10));
		}

		[Fact]
		public void TestFlexDirectionRowReverse()
		{
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, };
			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 10, };
			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 10, };

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
					view1,
					view2,
				},

				Direction = FlexDirection.RowReverse,
			};

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(90, 0, 10, 100));
			Assert.Equal(view1.Bounds, new Rect(80, 0, 10, 100));
			Assert.Equal(view2.Bounds, new Rect(70, 0, 10, 100));
		}
	}
}