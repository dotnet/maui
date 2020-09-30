using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
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
			Assert.AreEqual("[Color: A=-1, R=-1, G=-1, B=-1, Hue=-1, Saturation=-1, Luminosity=-1]", solidColorBrush.Color.ToString(), "Color");
		}

		[Test]
		public void TestConstructorUsingColor()
		{
			SolidColorBrush solidColorBrush = new SolidColorBrush(Color.Red);
			Assert.AreEqual("[Color: A=1, R=1, G=0, B=0, Hue=1, Saturation=1, Luminosity=0.5]", solidColorBrush.Color.ToString(), "Color");
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
		public void TestSolidColorBrushFromColor()
		{
			SolidColorBrush solidColorBrush = new SolidColorBrush(Color.Red);
			Assert.IsNotNull(solidColorBrush.Color);
			Assert.AreEqual("#FFFF0000", solidColorBrush.Color.ToHex());
		}

		[Test]
		public void TestDefaultBrushes()
		{
			SolidColorBrush black = Brush.Black;
			Assert.IsNotNull(black.Color);
			Assert.AreEqual("#FF000000", black.Color.ToHex());

			SolidColorBrush white = Brush.White;
			Assert.IsNotNull(white.Color);
			Assert.AreEqual("#FFFFFFFF", white.Color.ToHex());
		}
	}
}