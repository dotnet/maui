using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class EasingTests : BaseTestFixture
	{
		[Test]
		public void Linear ([Range (0, 10)] double input)
		{
			Assert.AreEqual (input, Easing.Linear.Ease (input));
		}

		[Test]
		public void AllRunFromZeroToOne ([Values (0.0, 1.0)] double val)
		{
			const double epsilon = 0.001;
			Assert.True (Math.Abs (val - Easing.Linear.Ease (val)) < epsilon);
			Assert.True (Math.Abs (val - Easing.BounceIn.Ease (val)) < epsilon);
			Assert.True (Math.Abs (val - Easing.BounceOut.Ease (val)) < epsilon);
			Assert.True (Math.Abs (val - Easing.CubicIn.Ease (val)) < epsilon);
			Assert.True (Math.Abs (val - Easing.CubicInOut.Ease (val)) < epsilon);
			Assert.True (Math.Abs (val - Easing.CubicOut.Ease (val)) < epsilon);
			Assert.True (Math.Abs (val - Easing.SinIn.Ease (val)) < epsilon);
			Assert.True (Math.Abs (val - Easing.SinInOut.Ease (val)) < epsilon);
			Assert.True (Math.Abs (val - Easing.SinOut.Ease (val)) < epsilon);
			Assert.True (Math.Abs (val - Easing.SpringIn.Ease (val)) < epsilon);
			Assert.True (Math.Abs (val - Easing.SpringOut.Ease (val)) < epsilon);
		}
	}
}
