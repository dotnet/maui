using System;
using System.Reflection;
using System.Xml;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class FooMarkupExtension : IMarkupExtension
	{
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return "Foo";
		}
	}

	public class AppendMarkupExtension : IMarkupExtension
	{
		public object Value0 { get; set; }
		public object Value1 { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return Value0.ToString() + Value1.ToString();
		}
	}

	public class AccessServiceProviderExtension : IMarkupExtension
	{
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			var result = "";
			if (serviceProvider != null)
			{
				var targetValueProvider = serviceProvider.GetService(typeof(IProvideValueTarget));
				result += targetValueProvider != null;
				var xamlType = serviceProvider.GetService(typeof(IXamlTypeResolver));
				result += xamlType != null;
				var rootObject = serviceProvider.GetService(typeof(IRootObjectProvider));
				result += rootObject != null;
			}
			return result;
		}
	}

	public class ColorMarkup : IMarkupExtension
	{
		public byte R { get; set; }
		public byte G { get; set; }
		public byte B { get; set; }

		public ColorMarkup()
		{
		}

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return Color.FromRgb(R, G, B);
		}
	}

	public class FuuExtension : IMarkupExtension
	{
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return "FuuExtension";
		}
	}

	public class Fuu : IMarkupExtension
	{
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return "Fuu";
		}
	}

	public class BaaExtension : IMarkupExtension
	{
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return "BaaExtension";
		}
	}

	[TestFixture]
	public class MarkupExtensionTests : BaseTestFixture
	{
		IXamlTypeResolver typeResolver;

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			var nsManager = new XmlNamespaceManager(new NameTable());
			nsManager.AddNamespace("local", "clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests");
			nsManager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");

			typeResolver = new Internals.XamlTypeResolver(nsManager, XamlParser.GetElementType, Assembly.GetCallingAssembly());
		}

		[Test]
		public void TestSimpleExtension()
		{
			var markupString = "{local:FooMarkupExtension}";
			var serviceProvider = new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			};
			var result = (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider);

			Assert.That(result, Is.InstanceOf<string>());
			Assert.AreEqual("Foo", result);
		}

		[Test]
		public void TestExtensionWithParameters()
		{
			var markupString = "{local:AppendMarkupExtension Value0=Foo, Value1=Bar}";
			var serviceProvider = new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			};
			var result = (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider);

			Assert.That(result, Is.InstanceOf<string>());
			Assert.AreEqual("FooBar", result);
		}

		[Test]
		public void TestServiceProvider()
		{
			var markupString = "{local:AccessServiceProviderExtension}";
			var serviceProvider = new Internals.XamlServiceProvider(null, null)
			{
				IProvideValueTarget = new Internals.XamlValueTargetProvider(null, null, null, null),
				IXamlTypeResolver = typeResolver,
				IRootObjectProvider = new Internals.XamlRootObjectProvider(null),
			};

			var result = (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider);

			Assert.That(result, Is.InstanceOf<string>());
			Assert.AreEqual("TrueTrueTrue", result);
		}

		[Test]
		public void TestInXaml()
		{
			var xaml = @"
			<Label 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests""
				Text=""{local:AppendMarkupExtension Value0=Foo, Value1=Bar}""
			/>";

			var label = new Label();
			label.LoadFromXaml(xaml);
			Assert.AreEqual("FooBar", label.Text.ToString());
		}

		[Test]
		public void TestMarkupExtensionInDefaultNamespace()
		{
			var xaml = @"
			<forms:Label 
				xmlns=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:forms=""http://schemas.microsoft.com/dotnet/2021/maui""
				Text=""{AppendMarkupExtension Value0=Foo, Value1=Bar}""
			/>";

			var label = new Label();
			label.LoadFromXaml(xaml);
			Assert.AreEqual("FooBar", label.Text.ToString());
		}

		[Test]
		public void TestDocumentationCode()
		{
			var xaml = @"
			<Label
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests""
				TextColor=""{local:ColorMarkup R=100, G=80, B=60}""/>";

			var label = new Label().LoadFromXaml(xaml);
			Assert.AreEqual(Color.FromRgb(100, 80, 60), label.TextColor);
		}

		[Test]
		public void TestLookupWithSuffix()
		{
			var markupString = "{local:Baa}";
			var serviceProvider = new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			};
			var result = (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider);

			Assert.That(result, Is.InstanceOf<string>());
			Assert.AreEqual("BaaExtension", result);
		}

		[Test]
		public void TestLookupOrder()
		{
			//The order of lookup is to look for the Extension-suffixed class name first and then look for the class name without the Extension suffix.
			var markupString = "{local:Fuu}";
			var serviceProvider = new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			};
			var result = (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider);

			Assert.That(result, Is.InstanceOf<string>());
			Assert.AreEqual("FuuExtension", result);
		}

		[Test]
		public void ThrowOnMarkupExtensionNotFound()
		{
			var markupString = "{local:Missing}";
			var serviceProvider = new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			};
			Assert.Throws<XamlParseException>(() => (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider));
		}
	}
}