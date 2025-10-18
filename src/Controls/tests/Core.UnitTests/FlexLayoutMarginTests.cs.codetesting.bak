using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using FlexLayout = Microsoft.Maui.Controls.Compatibility.FlexLayout;

	public class FlexLayoutMarginTests : BaseTestFixture
	{
		[Fact]
		public void TestMarginLeft()
		{
			var view0 = MockPlatformSizeService.Sub<View>(width: 10, margin: new(10, 0, 0, 0));
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
				},
				Direction = FlexDirection.Row,
			};

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(10, 0, 10, 100));
		}

		[Fact]
		public void TestMarginTop()
		{
			var view0 = MockPlatformSizeService.Sub<View>(height: 10, margin: new(0, 10, 0, 0));
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
				},

				Direction = FlexDirection.Column,
			};

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 10, 100, 10));
		}

		[Fact]
		public void TestMarginRight()
		{
			var view0 = MockPlatformSizeService.Sub<View>(width: 10, margin: new(0, 0, 10, 0));
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
				},

				Direction = FlexDirection.Row,
				JustifyContent = FlexJustify.End,
			};

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(80, 0, 10, 100));
		}

		[Fact]
		public void TestMarginBottom()
		{
			var view0 = MockPlatformSizeService.Sub<View>(height: 10, margin: new(0, 0, 0, 10));
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
				},

				Direction = FlexDirection.Column,
				JustifyContent = FlexJustify.End,
			};

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 80, 100, 10));
		}

		[Fact]
		public void TestMarginAndFlexRow()
		{
			var view0 = MockPlatformSizeService.Sub<View>(margin: new(10, 0, 10, 0));
			FlexLayout.SetGrow(view0, 1);
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
				},

				Direction = FlexDirection.Row,
			};
			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(10, 0, 80, 100));
		}

		[Fact]
		public void TestMarginAndFlexColumn()
		{
			var view0 = MockPlatformSizeService.Sub<View>(margin: new(0, 10, 0, 10));
			FlexLayout.SetGrow(view0, 1);
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
				},

				Direction = FlexDirection.Column,
			};
			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 10, 100, 80));
		}

		[Fact]
		public void TestMarginAndStretchRow()
		{
			var view0 = MockPlatformSizeService.Sub<View>(margin: new(0, 10, 0, 10));
			FlexLayout.SetGrow(view0, 1);
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
				},

				Direction = FlexDirection.Row,
			};

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 10, 100, 80));
		}

		[Fact]
		public void TestMarginAndStretchColumn()
		{
			var view0 = MockPlatformSizeService.Sub<View>(margin: new(10, 0, 10, 0));
			FlexLayout.SetGrow(view0, 1);
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
				},
				Direction = FlexDirection.Column,
			};

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(10, 0, 80, 100));
		}

		[Fact]
		public void TestMarginWithSiblingRow()
		{
			static SizeRequest GetSize(VisualElement _, double w, double h) => new(new(0, 0));

			var view0 = MockPlatformSizeService.Sub<View>(GetSize, margin: new(0, 0, 10, 0));
			FlexLayout.SetGrow(view0, 1);
			var view1 = MockPlatformSizeService.Sub<View>(GetSize);
			FlexLayout.SetGrow(view1, 1);
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
					view1,
				},

				Direction = FlexDirection.Row,
			};

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 45, 100));
			Assert.Equal(view1.Bounds, new Rect(55, 0, 45, 100));
		}

		[Fact]
		public void TestMarginWithSiblingColumn()
		{
			var view0 = MockPlatformSizeService.Sub<View>(margin: new(0, 0, 0, 10));
			FlexLayout.SetGrow(view0, 1);
			var view1 = MockPlatformSizeService.Sub<View>();
			FlexLayout.SetGrow(view1, 1);

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Children = {
					view0,
					view1,
				},

				Direction = FlexDirection.Column,
			};

			layout.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(layout.Bounds, new Rect(0, 0, 100, 100));
			Assert.Equal(view0.Bounds, new Rect(0, 0, 100, 45));
			Assert.Equal(view1.Bounds, new Rect(0, 55, 100, 45));
		}
	}
}