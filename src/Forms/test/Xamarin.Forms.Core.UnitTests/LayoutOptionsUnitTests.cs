using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class LayoutOptionsUnitTests : BaseTestFixture
	{
		[Test]
		public void TestTypeConverter()
		{
			var converter = new LayoutOptionsConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));
			Assert.AreEqual(LayoutOptions.Center, converter.ConvertFromInvariantString("LayoutOptions.Center"));
			Assert.AreEqual(LayoutOptions.Center, converter.ConvertFromInvariantString("Center"));
			Assert.AreNotEqual(LayoutOptions.CenterAndExpand, converter.ConvertFromInvariantString("Center"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("foo"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("foo.bar"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("foo.bar.baz"));
		}
	}
}