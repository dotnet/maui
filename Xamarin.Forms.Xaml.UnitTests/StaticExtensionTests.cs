using System;
using NUnit.Framework;
using System.Xml;

using Xamarin.Forms.Core.UnitTests;
using System.Reflection;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TestFixture]
	public class StaticExtensionTests : BaseTestFixture
	{
		IXamlTypeResolver typeResolver;

		[SetUp]
		public override void Setup ()
		{
			base.Setup ();
			var nsManager = new XmlNamespaceManager (new NameTable ());
			nsManager.AddNamespace ("local", "clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests");
			nsManager.AddNamespace ("x", "http://schemas.microsoft.com/winfx/2006/xaml");

			typeResolver = new Internals.XamlTypeResolver (nsManager, XamlParser.GetElementType, Assembly.GetCallingAssembly ());
		}

		[Test]
		public void TestxStatic ()
		{
			//{x:Static Member=prefix:typeName.staticMemberName}
			//{x:Static prefix:typeName.staticMemberName}

			//The code entity that is referenced must be one of the following:
			// - A constant
			// - A static property
			// - A field
			// - An enumeration value
			// All other cases should throw

			var serviceProvider = new Internals.XamlServiceProvider (null, null) { 
				IXamlTypeResolver = typeResolver,
			};

			//Static property
			var markupString = @"{x:Static Member=""local:MockxStatic.MockStaticProperty""}";
			Assert.AreEqual ("Property", (new MarkupExtensionParser ()).ParseExpression (ref markupString, serviceProvider));

			//constant
			markupString = @"{x:Static Member=""local:MockxStatic.MockConstant""}";
			Assert.AreEqual ("Constant", (new MarkupExtensionParser ()).ParseExpression (ref markupString, serviceProvider));

			//field
			markupString = @"{x:Static Member=""local:MockxStatic.MockField""}";
			Assert.AreEqual ("Field", (new MarkupExtensionParser ()).ParseExpression (ref markupString, serviceProvider));

			//enum
			markupString = @"{x:Static Member=""local:MockEnum.Second""}";
			Assert.AreEqual (MockEnum.Second, (new MarkupExtensionParser ()).ParseExpression (ref markupString, serviceProvider));

			//throw on InstanceProperty
			markupString = @"{x:Static Member=""local:MockxStatic.InstanceProperty""}";
			Assert.Throws<XamlParseException> (()=> (new MarkupExtensionParser ()).ParseExpression (ref markupString, serviceProvider));

			//quotes are optional
			markupString = @"{x:Static Member=local:MockxStatic.MockStaticProperty}";
			Assert.AreEqual ("Property", (new MarkupExtensionParser ()).ParseExpression (ref markupString, serviceProvider));

			//Member is optional
			markupString = @"{x:Static local:MockxStatic.MockStaticProperty}";
			Assert.AreEqual ("Property", (new MarkupExtensionParser ()).ParseExpression (ref markupString, serviceProvider));
		}
	}
}