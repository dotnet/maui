using System;
using System.Reflection;
using System.Xml;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{

	[Collection("Xaml Inflation")]
	public class TypeExtensionTests : BaseTestFixture
	{
		IXamlTypeResolver typeResolver;
		Internals.XamlServiceProvider serviceProvider;

		public TypeExtensionTests()
		{
			var nsManager = new XmlNamespaceManager(new NameTable());
			nsManager.AddNamespace("", "http://schemas.microsoft.com/dotnet/2021/maui");
			nsManager.AddNamespace("local", "clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests");
			nsManager.AddNamespace("sys", "clr-namespace:System;assembly=mscorlib");
			nsManager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");

			typeResolver = new Internals.XamlTypeResolver(nsManager, XamlParser.GetElementType, Assembly.GetCallingAssembly());

			serviceProvider = new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			};
		}

		[Fact]
		public void TestxType()
		{
			var markupString = @"{x:Type sys:String}";
			Assert.Equal(typeof(string), (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider));
		}

		[Fact]
		public void TestWithoutPrefix()
		{
			var markupString = @"{x:Type Grid}";
			Assert.Equal(typeof(Grid), (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider));
		}

		[Fact]
		public void TestWithExplicitTypeName()
		{
			var markupString = @"{x:Type TypeName=sys:String}";
			Assert.Equal(typeof(string), (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider));
		}
	}
}