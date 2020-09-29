using System;
using System.Reflection;
using System.Xml;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{

	[TestFixture]
	public class TypeExtensionTests : BaseTestFixture
	{
		IXamlTypeResolver typeResolver;
		Internals.XamlServiceProvider serviceProvider;

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			var nsManager = new XmlNamespaceManager(new NameTable());
			nsManager.AddNamespace("", "http://xamarin.com/schemas/2014/forms");
			nsManager.AddNamespace("local", "clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests");
			nsManager.AddNamespace("sys", "clr-namespace:System;assembly=mscorlib");
			nsManager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");

			typeResolver = new Internals.XamlTypeResolver(nsManager, XamlParser.GetElementType, Assembly.GetCallingAssembly());

			serviceProvider = new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			};
		}

		[Test]
		public void TestxType()
		{
			var markupString = @"{x:Type sys:String}";
			Assert.AreEqual(typeof(string), (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider));
		}

		[Test]
		public void TestWithoutPrefix()
		{
			var markupString = @"{x:Type Grid}";
			Assert.AreEqual(typeof(Grid), (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider));
		}

		[Test]
		public void TestWithExplicitTypeName()
		{
			var markupString = @"{x:Type TypeName=sys:String}";
			Assert.AreEqual(typeof(string), (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider));
		}
	}
}