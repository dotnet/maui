using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class BoxViewUnitTests : BaseTestFixture
	{
		[Test]
		public void TestConstructor()
		{
			var box = new BoxView
			{
				Color = new Color(0.2f, 0.3f, 0.4f),
				WidthRequest = 20,
				HeightRequest = 30,
				IsPlatformEnabled = true,
			};

			Assert.AreEqual(new Color(0.2f, 0.3f, 0.4f), box.Color);
			var request = box.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request;
			Assert.AreEqual(20, request.Width);
			Assert.AreEqual(30, request.Height);
		}

		[Test]
		public void DefaultSize()
		{
			var box = new BoxView
			{
				IsPlatformEnabled = true,
			};

			var request = box.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request;
			Assert.AreEqual(40, request.Width);
			Assert.AreEqual(40, request.Height);
		}
	}
}
