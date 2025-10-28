using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{	public class NullExtensionTests : BaseTestFixture
	{
		[Fact]
		public void TestxNull()
		{
			var markupString = "{x:Null}";
			var serviceProvider = new Internals.XamlServiceProvider(null, null);
			var result = (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider);

			Assert.Null(result);
		}
	}
}
