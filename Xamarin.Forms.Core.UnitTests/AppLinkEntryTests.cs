using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class AppLinkEntryTests : BaseTestFixture
	{

		[Test]
		public void KeyValuesTest()
		{
			var entry = new AppLinkEntry();

			entry.KeyValues.Add("contentType", "GalleryPage");
			entry.KeyValues.Add("companyName", "Xamarin");
			Assert.AreEqual(entry.KeyValues.Count, 2);
		}


		[Test]
		public void FromUriTest()
		{
			var uri = new Uri("http://foo.com");

			var entry = AppLinkEntry.FromUri(uri);

			Assert.AreEqual(uri, entry.AppLinkUri);
		}

		[Test]
		public void ToStringTest()
		{
			var str = "http://foo.com";
			var uri = new Uri(str);

			var entry = new AppLinkEntry { AppLinkUri = uri };

			Assert.AreEqual(uri.ToString(), entry.ToString());
		}
	}
}