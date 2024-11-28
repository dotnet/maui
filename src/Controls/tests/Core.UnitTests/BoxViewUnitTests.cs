using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class BoxViewUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			var box = new BoxView
			{
				Color = new Color(0.2f, 0.3f, 0.4f),
				WidthRequest = 20,
				HeightRequest = 30,
				IsPlatformEnabled = true,
			};

			Assert.Equal(new Color(0.2f, 0.3f, 0.4f), box.Color);
			var request = box.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request;
			Assert.Equal(20, request.Width);
			Assert.Equal(30, request.Height);
		}

		[Fact]
		public void DefaultSize()
		{
			var box = new BoxView
			{
				IsPlatformEnabled = true,
			};

			var request = box.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request;
			Assert.Equal(40, request.Width);
			Assert.Equal(40, request.Height);
		}
	}
}
