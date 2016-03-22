using System;
using NUnit.Framework;

using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class BoxViewUnitTests : BaseTestFixture
	{
		[Test]
		public void TestConstructor ()
		{
			var box = new BoxView {
				Color = new Color (0.2, 0.3, 0.4), 
				WidthRequest=20, 
				HeightRequest=30,
				IsPlatformEnabled = true,
				Platform = new UnitPlatform ()
			};

			Assert.AreEqual (new Color (0.2, 0.3, 0.4), box.Color);
			var request = box.GetSizeRequest (double.PositiveInfinity, double.PositiveInfinity).Request;
			Assert.AreEqual (20, request.Width);
			Assert.AreEqual (30, request.Height);
		}

		[Test]
		public void DefaultSize ()
		{
			var box = new BoxView {
				IsPlatformEnabled = true,
				Platform = new UnitPlatform ()
			};

			var request = box.GetSizeRequest (double.PositiveInfinity, double.PositiveInfinity).Request;
			Assert.AreEqual (40, request.Width);
			Assert.AreEqual (40, request.Height);
		}
	}	
}
