using System;
using System.Globalization;
using System.Threading;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using FlexLayout = Microsoft.Maui.Controls.Compatibility.FlexLayout;


	public class FlexOrderTests : BaseTestFixture
	{
		[Fact]
		public void TestOrderingElements()
		{
			var label0 = MockPlatformSizeService.Sub<Label>();
			var label1 = MockPlatformSizeService.Sub<Label>();
			var label2 = MockPlatformSizeService.Sub<Label>();
			var label3 = MockPlatformSizeService.Sub<Label>();

			FlexLayout.SetOrder(label3, 0);
			FlexLayout.SetOrder(label2, 1);
			FlexLayout.SetOrder(label1, 2);
			FlexLayout.SetOrder(label0, 3);

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Column,
				Children = {
					label0,
					label1,
					label2,
					label3
				}
			};

			layout.Layout(new Rect(0, 0, 912, 912));

			Assert.Equal(layout.Bounds, new Rect(0, 0, 912, 912));
			Assert.Equal(label3.Bounds, new Rect(0, 0, 912, 20));
			Assert.Equal(label2.Bounds, new Rect(0, 20, 912, 20));
			Assert.Equal(label1.Bounds, new Rect(0, 40, 912, 20));
			Assert.Equal(label0.Bounds, new Rect(0, 60, 912, 20));
		}

		[Fact]
		public void TestReverseOrderingElements()
		{
			// Children inserted in order 0..3, but Order values are reversed (3, 2, 1, 0)
			// so layout should place label0 last and label3 first
			var label0 = MockPlatformSizeService.Sub<Label>();
			var label1 = MockPlatformSizeService.Sub<Label>();
			var label2 = MockPlatformSizeService.Sub<Label>();
			var label3 = MockPlatformSizeService.Sub<Label>();

			FlexLayout.SetOrder(label0, 3);
			FlexLayout.SetOrder(label1, 2);
			FlexLayout.SetOrder(label2, 1);
			FlexLayout.SetOrder(label3, 0);

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Column,
				Children = { label0, label1, label2, label3 }
			};

			layout.Layout(new Rect(0, 0, 912, 912));

			// label3 (Order=0), label2 (Order=1), label1 (Order=2), label0 (Order=3)
			Assert.Equal(new Rect(0, 0, 912, 20), label3.Bounds);
			Assert.Equal(new Rect(0, 20, 912, 20), label2.Bounds);
			Assert.Equal(new Rect(0, 40, 912, 20), label1.Bounds);
			Assert.Equal(new Rect(0, 60, 912, 20), label0.Bounds);
		}

		[Fact]
		public void TestStableSortPreservesInsertionOrder()
		{
			// When multiple children share the same Order value,
			// they must keep their original insertion order (stable sort)
			var label0 = MockPlatformSizeService.Sub<Label>();
			var label1 = MockPlatformSizeService.Sub<Label>();
			var label2 = MockPlatformSizeService.Sub<Label>();
			var label3 = MockPlatformSizeService.Sub<Label>();

			// label0 has high Order, label1-3 share Order=0 → stability matters
			FlexLayout.SetOrder(label0, 1);
			FlexLayout.SetOrder(label1, 0);
			FlexLayout.SetOrder(label2, 0);
			FlexLayout.SetOrder(label3, 0);

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Column,
				Children = { label0, label1, label2, label3 }
			};

			layout.Layout(new Rect(0, 0, 912, 912));

			// label1, label2, label3 all have Order=0 → preserve insertion order
			// label0 has Order=1 → comes last
			Assert.Equal(new Rect(0, 0, 912, 20), label1.Bounds);
			Assert.Equal(new Rect(0, 20, 912, 20), label2.Bounds);
			Assert.Equal(new Rect(0, 40, 912, 20), label3.Bounds);
			Assert.Equal(new Rect(0, 60, 912, 20), label0.Bounds);
		}

		[Fact]
		public void TestNegativeOrderValues()
		{
			// Negative Order values should sort before zero
			var label0 = MockPlatformSizeService.Sub<Label>();
			var label1 = MockPlatformSizeService.Sub<Label>();
			var label2 = MockPlatformSizeService.Sub<Label>();

			FlexLayout.SetOrder(label0, 1);
			FlexLayout.SetOrder(label1, 0);
			FlexLayout.SetOrder(label2, -1);

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Column,
				Children = { label0, label1, label2 }
			};

			layout.Layout(new Rect(0, 0, 912, 912));

			// label2 (Order=-1), label1 (Order=0), label0 (Order=1)
			Assert.Equal(new Rect(0, 0, 912, 20), label2.Bounds);
			Assert.Equal(new Rect(0, 20, 912, 20), label1.Bounds);
			Assert.Equal(new Rect(0, 40, 912, 20), label0.Bounds);
		}

		[Fact]
		public void TestOrderWithRowDirection()
		{
			// Verify ordering works in Row direction too (horizontal layout)
			var label0 = MockPlatformSizeService.Sub<Label>();
			var label1 = MockPlatformSizeService.Sub<Label>();
			var label2 = MockPlatformSizeService.Sub<Label>();

			FlexLayout.SetOrder(label0, 2);
			FlexLayout.SetOrder(label1, 0);
			FlexLayout.SetOrder(label2, 1);

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row,
				Children = { label0, label1, label2 }
			};

			layout.Layout(new Rect(0, 0, 912, 912));

			// label1 (Order=0), label2 (Order=1), label0 (Order=2)
			// In row direction, x position changes
			Assert.Equal(0d, label1.Bounds.X);
			Assert.True(label2.Bounds.X > label1.Bounds.X);
			Assert.True(label0.Bounds.X > label2.Bounds.X);
		}
	}
}
