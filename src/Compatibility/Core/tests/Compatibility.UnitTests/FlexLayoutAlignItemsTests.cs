using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using FlexLayout = Microsoft.Maui.Controls.Compatibility.FlexLayout;


	public class FlexLayoutAlignItemsTests : BaseTestFixture
	{
		[Fact]
		public void TestAlignItemsStretch()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				Direction = FlexDirection.Column,
			};
			var view0 = new View { IsPlatformEnabled = true, HeightRequest = 10, };
			layout.Children.Add(view0);

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 100, 10));
		}

		[Fact]
		public void TestAlignItemsCenter()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				AlignItems = FlexAlignItems.Center,
				Direction = FlexDirection.Column,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, HeightRequest = 10 };
			layout.Children.Add(view0);

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(45, 0, 10, 10));
		}

		[Fact]
		public void TestAlignItemsFlexStart()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				AlignItems = FlexAlignItems.Start,
				Direction = FlexDirection.Column,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, HeightRequest = 10 };
			layout.Children.Add(view0);

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 10, 10));
		}

		[Fact]
		public void TestAlignItemsFlexEnd()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				AlignItems = FlexAlignItems.End,
				Direction = FlexDirection.Column,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, HeightRequest = 10 };
			layout.Children.Add(view0);

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(90, 0, 10, 10));
		}
	}
}