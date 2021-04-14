using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class SolidColorBrushTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
		}

		[Test]
		public void TestConstructor()
		{
			SolidColorBrush solidColorBrush = new SolidColorBrush();
			Assert.Null(solidColorBrush.Color, "Color");
		}

		[Test]
		public void TestConstructorUsingColor()
		{
			SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Red);
			Assert.That(solidColorBrush.Color, Is.EqualTo(Colors.Red));
		}

		[Test]
		public void TestEmptySolidColorBrush()
		{
			SolidColorBrush solidColorBrush = new SolidColorBrush();
			Assert.AreEqual(true, solidColorBrush.IsEmpty, "IsEmpty");

			SolidColorBrush red = Brush.Red;
			Assert.AreEqual(false, red.IsEmpty, "IsEmpty");
		}

		[Test]
		public void TestNullOrEmptySolidColorBrush()
		{
			SolidColorBrush nullSolidColorBrush = null;
			Assert.AreEqual(true, Brush.IsNullOrEmpty(nullSolidColorBrush), "IsNullOrEmpty");

			SolidColorBrush emptySolidColorBrush = new SolidColorBrush();
			Assert.AreEqual(true, Brush.IsNullOrEmpty(emptySolidColorBrush), "IsNullOrEmpty");

			SolidColorBrush solidColorBrush = Brush.Yellow;
			Assert.AreEqual(false, Brush.IsNullOrEmpty(solidColorBrush), "IsNullOrEmpty");
		}

		[Test]
		public void TestDefaultBrushes()
		{
			SolidColorBrush black = Brush.Black;
			Assert.IsNotNull(black.Color);
			Assert.That(black.Color, Is.EqualTo(Colors.Black));

			SolidColorBrush white = Brush.White;
			Assert.IsNotNull(white.Color);
			Assert.That(white.Color, Is.EqualTo(Colors.White));
		}
	}
}