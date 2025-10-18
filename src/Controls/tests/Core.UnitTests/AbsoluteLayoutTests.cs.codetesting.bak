using System;
using System.Collections.Generic;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AbsoluteLayoutTests : BaseTestFixture
	{
		public AbsoluteLayoutTests()
		{
			DeviceDisplay.SetCurrent(new MockDeviceDisplay());
		}

		[Fact]
		public void Constructor()
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			Assert.Empty(abs.Children);

			var sizeReq = abs.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);
			Assert.Equal(Size.Zero, sizeReq.Request);
			Assert.Equal(Size.Zero, sizeReq.Minimum);
		}

		[Fact]
		public void AbsolutePositionAndSizeUsingRectangle()
		{
			var abs = new Compatibility.AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = MockPlatformSizeService.Sub<View>();

			abs.Children.Add(child, new Rect(10, 20, 30, 40));

			abs.Layout(new Rect(0, 0, 100, 100));

			Assert.Equal(new Rect(10, 20, 30, 40), child.Bounds);
		}


		[Fact]
		public void AbsolutePositionRelativeSize()
		{
			var abs = new Compatibility.AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = MockPlatformSizeService.Sub<View>();

			abs.Children.Add(child, new Rect(10, 20, 0.4, 0.5), AbsoluteLayoutFlags.SizeProportional);

			abs.Layout(new Rect(0, 0, 100, 100));

			Assert.Equal(10, child.X);
			Assert.Equal(20, child.Y);
			Assert.Equal(40, child.Width, 4);
			Assert.Equal(50, child.Height, 4);
		}

		[Theory]
		[InlineData(30, 40, 0.2, 0.3)]
		[InlineData(35, 45, 0.5, 0.5)]
		[InlineData(35, 45, 0, 0)]
		[InlineData(35, 45, 1, 1)]
		public void RelativePositionAbsoluteSize(double width, double height, double relX, double relY)
		{
			var abs = new Compatibility.AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = MockPlatformSizeService.Sub<View>();

			abs.Children.Add(child, new Rect(relX, relY, width, height), AbsoluteLayoutFlags.PositionProportional);

			abs.Layout(new Rect(0, 0, 100, 100));

			double expectedX = Math.Round((100 - width) * relX);
			double expectedY = Math.Round((100 - height) * relY);
			Assert.Equal(expectedX, child.X, 4);
			Assert.Equal(expectedY, child.Y, 4);
			Assert.Equal(width, child.Width);
			Assert.Equal(height, child.Height);
		}

		public static IEnumerable<object[]> RelativeData()
		{
			return TestDataHelpers.Combinations(new List<double>() { 0.0, 0.2, 0.5, 1.0 });
		}

		[Theory]
		[MemberData(nameof(RelativeData))]
		public void RelativePositionRelativeSize(double relX, double relY, double relHeight, double relWidth)
		{
			var abs = new Compatibility.AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = MockPlatformSizeService.Sub<View>();
			abs.Children.Add(child, new Rect(relX, relY, relWidth, relHeight), AbsoluteLayoutFlags.All);
			abs.Layout(new Rect(0, 0, 100, 100));

			double expectedWidth = Math.Round(100 * relWidth);
			double expectedHeight = Math.Round(100 * relHeight);
			double expectedX = Math.Round((100 - expectedWidth) * relX);
			double expectedY = Math.Round((100 - expectedHeight) * relY);

			Assert.Equal(expectedX, child.X, 4);
			Assert.Equal(expectedY, child.Y, 4);
			Assert.Equal(expectedWidth, child.Width, 4);
			Assert.Equal(expectedHeight, child.Height, 4);
		}

		[Fact]
		public void SizeRequestWithNormalChild()
		{
			var abs = new Compatibility.AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = MockPlatformSizeService.Sub<View>();

			// ChildSizeReq == 100x20
			abs.Children.Add(child, new Rect(10, 20, 30, 40));

			var sizeReq = abs.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

			Assert.Equal(new Size(40, 60), sizeReq.Request);
			Assert.Equal(new Size(40, 60), sizeReq.Minimum);
		}

		[Fact]
		public void SizeRequestWithRelativePositionChild()
		{
			var abs = new Compatibility.AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = MockPlatformSizeService.Sub<View>();

			// ChildSizeReq == 100x20
			abs.Children.Add(child, new Rect(0.5, 0.5, 30, 40), AbsoluteLayoutFlags.PositionProportional);

			var sizeReq = abs.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

			Assert.Equal(new Size(30, 40), sizeReq.Request);
			Assert.Equal(new Size(30, 40), sizeReq.Minimum);
		}

		[Fact]
		public void SizeRequestWithRelativeChild()
		{
			var abs = new Compatibility.AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = MockPlatformSizeService.Sub<View>();

			// ChildSizeReq == 100x20
			abs.Children.Add(child, new Rect(0.5, 0.5, 0.5, 0.5), AbsoluteLayoutFlags.All);

			var sizeReq = abs.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

			Assert.Equal(new Size(200, 40), sizeReq.Request);
			Assert.Equal(new Size(0, 0), sizeReq.Minimum);
		}

		[Fact]
		public void SizeRequestWithRelativeSizeChild()
		{
			var abs = new Compatibility.AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = MockPlatformSizeService.Sub<View>();

			// ChildSizeReq == 100x20
			abs.Children.Add(child, new Rect(10, 20, 0.5, 0.5), AbsoluteLayoutFlags.SizeProportional);

			var sizeReq = abs.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

			Assert.Equal(new Size(210, 60), sizeReq.Request);
			Assert.Equal(new Size(10, 20), sizeReq.Minimum);
		}

		[Fact]
		public void MeasureInvalidatedFiresWhenFlagsChanged()
		{
			var abs = new Compatibility.AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = MockPlatformSizeService.Sub<View>();

			abs.Children.Add(child, new Rect(1, 1, 100, 100));

			bool fired = false;
			abs.MeasureInvalidated += (sender, args) => fired = true;

			AbsoluteLayout.SetLayoutFlags(child, AbsoluteLayoutFlags.PositionProportional);

			Assert.True(fired);
		}

		[Fact]
		public void MeasureInvalidatedFiresWhenBoundsChanged()
		{
			var abs = new Compatibility.AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = MockPlatformSizeService.Sub<View>();

			abs.Children.Add(child, new Rect(1, 1, 100, 100));

			bool fired = false;
			abs.MeasureInvalidated += (sender, args) => fired = true;

			AbsoluteLayout.SetLayoutBounds(child, new Rect(2, 2, 200, 200));

			Assert.True(fired);
		}

		[Theory]
		[InlineData("en-US"), InlineData("tr-TR")]
		public void TestBoundsTypeConverter(string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

			var converter = new BoundsTypeConverter();

			Assert.True(converter.CanConvertFrom(typeof(string)));
			Assert.Equal(new Rect(3, 4, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), converter.ConvertFromInvariantString("3, 4"));
			Assert.Equal(new Rect(3, 4, 20, 30), converter.ConvertFromInvariantString("3, 4, 20, 30"));
			Assert.Equal(new Rect(3, 4, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), converter.ConvertFromInvariantString("3, 4, AutoSize, AutoSize"));
			Assert.Equal(new Rect(3, 4, AbsoluteLayout.AutoSize, 30), converter.ConvertFromInvariantString("3, 4, AutoSize, 30"));
			Assert.Equal(new Rect(3, 4, 20, AbsoluteLayout.AutoSize), converter.ConvertFromInvariantString("3, 4, 20, AutoSize"));

			var autoSize = "AutoSize";
			Assert.Equal(new Rect(3.3, 4.4, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), converter.ConvertFromInvariantString("3.3, 4.4, " + autoSize + ", AutoSize"));
			Assert.Equal(new Rect(3.3, 4.4, 5.5, 6.6), converter.ConvertFromInvariantString("3.3, 4.4, 5.5, 6.6"));
		}
	}
}
