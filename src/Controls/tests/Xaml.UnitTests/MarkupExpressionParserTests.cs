using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class MarkupExpressionParserTests : BaseTestFixture
	{
		IXamlTypeResolver typeResolver;
		MockDeviceInfo mockDeviceInfo;

		public static readonly string Foo = "Foo";

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			var nsManager = new XmlNamespaceManager(new NameTable());
			nsManager.AddNamespace("local", "clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests");
			nsManager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2009/xaml");
			typeResolver = new Internals.XamlTypeResolver(nsManager, XamlParser.GetElementType, Assembly.GetCallingAssembly());
		}

		[Test]
		public void BindingOnSelf()
		{
			var bindingString = "{Binding}";
			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});
			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual(Binding.SelfPath, ((Binding)binding).Path);
		}

		[TestCase("{Binding Foo}")]
		[TestCase("{Binding {x:Static local:MarkupExpressionParserTests.Foo}}")]
		public void BindingWithImplicitPath(string bindingString)
		{
			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo", ((Binding)binding).Path);
		}

		[Test]
		public void BindingWithPath()
		{
			var bindingString = "{Binding Path=Foo}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo", ((Binding)binding).Path);
		}

		[Test]
		public void BindingWithComposedPath()
		{
			var bindingString = "{Binding Path=Foo.Bar}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo.Bar", ((Binding)binding).Path);
		}

		[Test]
		public void BindingWithImplicitComposedPath()
		{
			var bindingString = "{Binding Path=Foo.Bar}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo.Bar", ((Binding)binding).Path);
		}

		class MockValueProvider : IProvideParentValues, IProvideValueTarget
		{
			public MockValueProvider(string key, object resource)
			{
				var rd = new ResourceDictionary {
					{key, resource}
				};

				ve = new VisualElement
				{
					Resources = rd,
				};
			}


			VisualElement ve;
			public IEnumerable<object> ParentObjects
			{
				get
				{
					yield return ve;
				}
			}

			public object TargetObject => null;

			public object TargetProperty { get; set; } = null;
		}

		[Test]
		public void BindingWithImplicitPathAndConverter()
		{
			var bindingString = "{Binding Foo, Converter={StaticResource Bar}}";
			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
				IProvideValueTarget = new MockValueProvider("Bar", new ReverseConverter()),
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo", ((Binding)binding).Path);
			Assert.NotNull(((Binding)binding).Converter);
			Assert.That(((Binding)binding).Converter, Is.InstanceOf<ReverseConverter>());
		}

		[Test]
		public void BindingWithPathAndConverter()
		{
			var bindingString = "{Binding Path=Foo, Converter={StaticResource Bar}}";
			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
				IProvideValueTarget = new MockValueProvider("Bar", new ReverseConverter()),
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo", ((Binding)binding).Path);
			Assert.NotNull(((Binding)binding).Converter);
			Assert.That(((Binding)binding).Converter, Is.InstanceOf<ReverseConverter>());
		}


		[Test]
		public void TestBindingMode()
		{
			var bindingString = "{Binding Foo, Mode=TwoWay}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo", ((Binding)binding).Path);
			Assert.AreEqual(BindingMode.TwoWay, ((Binding)binding).Mode);
		}

		[Test]
		public void BindingStringFormat()
		{
			var bindingString = "{Binding Foo, StringFormat=Bar}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});
			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo", ((Binding)binding).Path);
			Assert.AreEqual("Bar", ((Binding)binding).StringFormat);
		}

		[Test]
		public void BindingStringFormatWithEscapes()
		{
			var bindingString = "{Binding Foo, StringFormat='{}Hello {0}'}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo", ((Binding)binding).Path);
			Assert.AreEqual("Hello {0}", ((Binding)binding).StringFormat);
		}

		[Test]
		public void BindingStringFormatWithoutEscaping()
		{
			var bindingString = "{Binding Foo, StringFormat='{0,20}'}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo", ((Binding)binding).Path);
			Assert.AreEqual("{0,20}", ((Binding)binding).StringFormat);
		}

		[Test]
		public void BindingStringFormatNumeric()
		{
			var bindingString = "{Binding Foo, StringFormat=P2}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo", ((Binding)binding).Path);
			Assert.AreEqual("P2", ((Binding)binding).StringFormat);
		}

		[Test]
		public void BindingConverterParameter()
		{
			var bindingString = "{Binding Foo, ConverterParameter='Bar'}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo", ((Binding)binding).Path);
			Assert.AreEqual("Bar", ((Binding)binding).ConverterParameter);
		}

		[Test]
		public void BindingsCompleteString()
		{
			var bindingString = "{Binding Path=Foo.Bar, StringFormat='{}Qux, {0}', Converter={StaticResource Baz}, Mode=OneWayToSource}";
			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
				IProvideValueTarget = new MockValueProvider("Baz", new ReverseConverter()),
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.AreEqual("Foo.Bar", ((Binding)binding).Path);
			Assert.NotNull(((Binding)binding).Converter);
			Assert.That(((Binding)binding).Converter, Is.InstanceOf<ReverseConverter>());
			Assert.AreEqual(BindingMode.OneWayToSource, ((Binding)binding).Mode);
			Assert.AreEqual("Qux, {0}", ((Binding)binding).StringFormat);
		}

		[Test]
		public void BindingWithStaticConverter()
		{
			var bindingString = "{Binding Converter={x:Static local:ReverseConverter.Instance}}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			}) as Binding;

			Assert.NotNull(binding);
			Assert.AreEqual(".", binding.Path);
			Assert.That(binding.Converter, Is.TypeOf<ReverseConverter>());
		}

		public int FontSize { get; set; }

		[TestCase("{OnPlatform 20, Android=23}", "Android", 23)]
		[TestCase("{OnPlatform Android=20, iOS=25}", "iOS", 25)]
		[TestCase("{OnPlatform Android=20, MacCatalyst=25}", "MacCatalyst", 25)]
		[TestCase("{OnPlatform Android=20, Tizen=25}", "Tizen", 25)]
		[TestCase("{OnPlatform Android=20, WinUI=25}", "WinUI", 25)]
		[TestCase("{OnPlatform Android=20, UWP=25}", "WinUI", 25)]
		[TestCase("{OnPlatform Android=20, WinUI=25, UWP=20}", "WinUI", 25)]
		[TestCase("{OnPlatform Android=20, UWP=25}", "UWP", 25)]
		[TestCase("{OnPlatform 20}", "Android", 20)]
		[TestCase("{OnPlatform 20}", "iOS", 20)]
		[TestCase("{OnPlatform 20}", "Tizen", 20)]
		[TestCase("{OnPlatform 20}", "WinUI", 20)]
		[TestCase("{OnPlatform 20}", "UWP", 20)]
		[TestCase("{OnPlatform 20}", "Foo", 20)]
		[TestCase("{OnPlatform Android=23, Default=20}", "Foo", 20)]
		public void OnPlatformExtension(string markup, string platform, int expected)
		{
			mockDeviceInfo.Platform = DevicePlatform.Create(platform);

			var actual = (new MarkupExtensionParser()).ParseExpression(ref markup, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
				IProvideValueTarget = new MockValueProvider("foo", new object())
				{
					TargetProperty = GetType().GetProperty(nameof(FontSize))
				}
			});

			Assert.AreEqual(expected, actual);
		}

		[TestCase("{OnIdiom Phone=23, Tablet=25, Default=20}", "Phone", 23)]
		[TestCase("{OnIdiom Phone=23, Tablet=25, Default=20}", "Tablet", 25)]
		[TestCase("{OnIdiom 20, Phone=23, Tablet=25}", "Desktop", 20)]
		[TestCase("{OnIdiom Phone=23, Tablet=25, Desktop=26, TV=30, Watch=10}", "Desktop", 26)]
		[TestCase("{OnIdiom Phone=23, Tablet=25, Desktop=26, TV=30, Watch=10}", "TV", 30)]
		[TestCase("{OnIdiom Phone=23, Tablet=25, Desktop=26, TV=30, Watch=10}", "Watch", 10)]
		[TestCase("{OnIdiom Phone=23}", "Desktop", default(int))]
		public void OnIdiomExtension(string markup, string idiom, int expected)
		{
			mockDeviceInfo.Idiom = DeviceIdiom.Create(idiom);

			var actual = (new MarkupExtensionParser()).ParseExpression(ref markup, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
				IProvideValueTarget = new MockValueProvider("foo", new object())
				{
					TargetProperty = GetType().GetProperty(nameof(FontSize))
				}
			});

			Assert.AreEqual(expected, actual);
		}

		[TestCase("{Binding")]
		[TestCase("{Binding 'Foo}")]
		[TestCase("{Binding Foo, Converter={StaticResource Bar}")]
		[TestCase("{Binding Foo, Converter={StaticResource Bar}?}")]
		public void InvalidExpressions(string expression)
		{
			var serviceProvider = new Internals.XamlServiceProvider(null, null);
			serviceProvider.IXamlTypeResolver = typeResolver;
			serviceProvider.IProvideValueTarget = new MockValueProvider("Bar", new ReverseConverter());
			Assert.Throws<XamlParseException>(() => (new MarkupExtensionParser()).ParseExpression(ref expression, serviceProvider));
		}
	}
}
