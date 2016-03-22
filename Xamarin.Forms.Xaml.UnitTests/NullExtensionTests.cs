using System;
using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TestFixture]
	public class NullExtensionTests : BaseTestFixture
	{
		[Test]
		public void TestxNull ()
		{
			var markupString = "{x:Null}";
			var serviceProvider = new Internals.XamlServiceProvider (null, null);
			var result = (new MarkupExtensionParser ()).ParseExpression (ref markupString, serviceProvider);

			Assert.IsNull (result);
		}
	}
}
