using NUnit.Framework;
using Rect = Xamarin.Forms.Shapes.Rectangle;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class RectTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();

			Device.SetFlags(new[] { ExperimentalFlags.ShapesExperimental });
		}

		[Test]
		public void RadiusCanBeSetFromStyle()
		{
			var rectangle = new Rect();

			Assert.AreEqual(0.0, rectangle.RadiusX);
			rectangle.SetValue(Rect.RadiusXProperty, 10.0, true);
			Assert.AreEqual(10.0, rectangle.RadiusX);

			Assert.AreEqual(0.0, rectangle.RadiusY);
			rectangle.SetValue(Rect.RadiusYProperty, 10.0, true);
			Assert.AreEqual(10.0, rectangle.RadiusY);
		}
	}
}