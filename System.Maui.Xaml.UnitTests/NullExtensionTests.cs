using System;
using NUnit.Framework;

using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
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
