using System;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class NumericExtensionsTests
	{
		[Test]
		public void InRange()
		{
			Assert.AreEqual(5, 5.Clamp(0, 10));
		}

		[Test]
		public void BelowMin()
		{
			Assert.AreEqual(5, 0.Clamp(5, 10));
		}

		[Test]
		public void AboveMax()
		{
			Assert.AreEqual(5, 10.Clamp(0, 5));
		}

		[Test]
		public void MinMaxWrong()
		{
			Assert.AreEqual(0, 10.Clamp(5, 0));
			Assert.AreEqual(0, 5.Clamp(10, 0));
			Assert.AreEqual(5, 0.Clamp(10, 5));
		}
	}
}