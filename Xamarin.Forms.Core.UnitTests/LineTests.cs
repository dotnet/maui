using NUnit.Framework;
using Xamarin.Forms.Shapes;

namespace Xamarin.Forms.Core.UnitTests
{
	public class LineTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();

			Device.SetFlags(new[] { ExperimentalFlags.ShapesExperimental });
		}

		[Test]
		public void XPointCanBeSetFromStyle()
		{
			var line = new Line();

			Assert.AreEqual(0.0, line.X1);
			line.SetValue(Line.X1Property, 1.0, true);
			Assert.AreEqual(1.0, line.X1);

			Assert.AreEqual(0.0, line.X2);
			line.SetValue(Line.X2Property, 100.0, true);
			Assert.AreEqual(100.0, line.X2);
		}

		[Test]
		public void YPointCanBeSetFromStyle()
		{
			var line = new Line();

			Assert.AreEqual(0.0, line.Y1);
			line.SetValue(Line.Y1Property, 1.0, true);
			Assert.AreEqual(1.0, line.Y1);

			Assert.AreEqual(0.0, line.Y2);
			line.SetValue(Line.Y2Property, 10.0, true);
			Assert.AreEqual(10.0, line.Y2);
		}
	}
}