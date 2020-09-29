using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class AbsoluteLayoutTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			var mockDeviceInfo = new TestDeviceInfo();
			Device.Info = mockDeviceInfo;
		}


		[Test]
		public void Constructor()
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			Assert.That(abs.Children, Is.Empty);

			var sizeReq = abs.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);
			Assert.AreEqual(Size.Zero, sizeReq.Request);
			Assert.AreEqual(Size.Zero, sizeReq.Minimum);
		}

		[Test]
		public void AbsolutePositionAndSizeUsingRectangle()
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = new View { IsPlatformEnabled = true };

			abs.Children.Add(child, new Rectangle(10, 20, 30, 40));

			abs.Layout(new Rectangle(0, 0, 100, 100));

			Assert.AreEqual(new Rectangle(10, 20, 30, 40), child.Bounds);
		}

		[Test]
		public void AbsolutePositionAndSizeUsingRect()
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = new View { IsPlatformEnabled = true };

			abs.Children.Add(child, new Rect(10, 20, 30, 40));

			abs.Layout(new Rect(0, 0, 100, 100));

			Assert.AreEqual(new Rect(10, 20, 30, 40), child.Bounds);
		}

		[Test]
		public void AbsolutePositionRelativeSize()
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = new View { IsPlatformEnabled = true };


			abs.Children.Add(child, new Rectangle(10, 20, 0.4, 0.5), AbsoluteLayoutFlags.SizeProportional);

			abs.Layout(new Rectangle(0, 0, 100, 100));

			Assert.That(child.X, Is.EqualTo(10));
			Assert.That(child.Y, Is.EqualTo(20));
			Assert.That(child.Width, Is.EqualTo(40).Within(0.0001));
			Assert.That(child.Height, Is.EqualTo(50).Within(0.0001));
		}

		[TestCase(30, 40, 0.2, 0.3)]
		[TestCase(35, 45, 0.5, 0.5)]
		[TestCase(35, 45, 0, 0)]
		[TestCase(35, 45, 1, 1)]
		public void RelativePositionAbsoluteSize(double width, double height, double relX, double relY)
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = new View { IsPlatformEnabled = true };

			abs.Children.Add(child, new Rectangle(relX, relY, width, height), AbsoluteLayoutFlags.PositionProportional);

			abs.Layout(new Rectangle(0, 0, 100, 100));

			double expectedX = Math.Round((100 - width) * relX);
			double expectedY = Math.Round((100 - height) * relY);
			Assert.That(child.X, Is.EqualTo(expectedX).Within(0.0001));
			Assert.That(child.Y, Is.EqualTo(expectedY).Within(0.0001));
			Assert.That(child.Width, Is.EqualTo(width));
			Assert.That(child.Height, Is.EqualTo(height));
		}

		[Test]
		public void RelativePositionRelativeSize([Values(0.0, 0.2, 0.5, 1.0)] double relX, [Values(0.0, 0.2, 0.5, 1.0)] double relY, [Values(0.0, 0.2, 0.5, 1.0)] double relHeight, [Values(0.0, 0.2, 0.5, 1.0)] double relWidth)
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = new View
			{
				IsPlatformEnabled = true
			};
			abs.Children.Add(child, new Rectangle(relX, relY, relWidth, relHeight), AbsoluteLayoutFlags.All);
			abs.Layout(new Rectangle(0, 0, 100, 100));

			double expectedWidth = Math.Round(100 * relWidth);
			double expectedHeight = Math.Round(100 * relHeight);
			double expectedX = Math.Round((100 - expectedWidth) * relX);
			double expectedY = Math.Round((100 - expectedHeight) * relY);
			Assert.That(child.X, Is.EqualTo(expectedX).Within(0.0001));
			Assert.That(child.Y, Is.EqualTo(expectedY).Within(0.0001));
			Assert.That(child.Width, Is.EqualTo(expectedWidth).Within(0.0001));
			Assert.That(child.Height, Is.EqualTo(expectedHeight).Within(0.0001));
		}

		[Test]
		public void SizeRequestWithNormalChild()
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = new View();

			// ChildSizeReq == 100x20
			abs.Children.Add(child, new Rectangle(10, 20, 30, 40));

			var sizeReq = abs.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);

			Assert.AreEqual(new Size(40, 60), sizeReq.Request);
			Assert.AreEqual(new Size(40, 60), sizeReq.Minimum);
		}

		[Test]
		public void SizeRequestWithRelativePositionChild()
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = new View();

			// ChildSizeReq == 100x20
			abs.Children.Add(child, new Rectangle(0.5, 0.5, 30, 40), AbsoluteLayoutFlags.PositionProportional);

			var sizeReq = abs.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);

			Assert.AreEqual(new Size(30, 40), sizeReq.Request);
			Assert.AreEqual(new Size(30, 40), sizeReq.Minimum);
		}

		[Test]
		public void SizeRequestWithRelativeChild()
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = new View
			{
				IsPlatformEnabled = true
			};

			// ChildSizeReq == 100x20
			abs.Children.Add(child, new Rectangle(0.5, 0.5, 0.5, 0.5), AbsoluteLayoutFlags.All);

			var sizeReq = abs.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);

			Assert.AreEqual(new Size(200, 40), sizeReq.Request);
			Assert.AreEqual(new Size(0, 0), sizeReq.Minimum);
		}

		[Test]
		public void SizeRequestWithRelativeSizeChild()
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = new View
			{
				IsPlatformEnabled = true
			};

			// ChildSizeReq == 100x20
			abs.Children.Add(child, new Rectangle(10, 20, 0.5, 0.5), AbsoluteLayoutFlags.SizeProportional);

			var sizeReq = abs.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);

			Assert.AreEqual(new Size(210, 60), sizeReq.Request);
			Assert.AreEqual(new Size(10, 20), sizeReq.Minimum);
		}

		[Test]
		public void MeasureInvalidatedFiresWhenFlagsChanged()
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = new View
			{
				IsPlatformEnabled = true
			};

			abs.Children.Add(child, new Rectangle(1, 1, 100, 100));

			bool fired = false;
			abs.MeasureInvalidated += (sender, args) => fired = true;

			AbsoluteLayout.SetLayoutFlags(child, AbsoluteLayoutFlags.PositionProportional);

			Assert.True(fired);
		}

		[Test]
		public void MeasureInvalidatedFiresWhenBoundsChanged()
		{
			var abs = new AbsoluteLayout
			{
				IsPlatformEnabled = true
			};

			var child = new View
			{
				IsPlatformEnabled = true
			};

			abs.Children.Add(child, new Rectangle(1, 1, 100, 100));

			bool fired = false;
			abs.MeasureInvalidated += (sender, args) => fired = true;

			AbsoluteLayout.SetLayoutBounds(child, new Rectangle(2, 2, 200, 200));

			Assert.True(fired);
		}

		[TestCase("en-US"), TestCase("tr-TR")]
		public void TestBoundsTypeConverter(string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

			var converter = new BoundsTypeConverter();

			Assert.IsTrue(converter.CanConvertFrom(typeof(string)));
			Assert.AreEqual(new Rectangle(3, 4, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), converter.ConvertFromInvariantString("3, 4"));
			Assert.AreEqual(new Rectangle(3, 4, 20, 30), converter.ConvertFromInvariantString("3, 4, 20, 30"));
			Assert.AreEqual(new Rectangle(3, 4, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), converter.ConvertFromInvariantString("3, 4, AutoSize, AutoSize"));
			Assert.AreEqual(new Rectangle(3, 4, AbsoluteLayout.AutoSize, 30), converter.ConvertFromInvariantString("3, 4, AutoSize, 30"));
			Assert.AreEqual(new Rectangle(3, 4, 20, AbsoluteLayout.AutoSize), converter.ConvertFromInvariantString("3, 4, 20, AutoSize"));

			var autoSize = "AutoSize";
			Assert.AreEqual(new Rectangle(3.3, 4.4, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), converter.ConvertFromInvariantString("3.3, 4.4, " + autoSize + ", AutoSize"));
			Assert.AreEqual(new Rectangle(3.3, 4.4, 5.5, 6.6), converter.ConvertFromInvariantString("3.3, 4.4, 5.5, 6.6"));
		}
	}
}