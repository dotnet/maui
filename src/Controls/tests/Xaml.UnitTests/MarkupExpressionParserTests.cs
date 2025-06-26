using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class MarkupExpressionParserTests : BaseTestFixture
	{
		IXamlTypeResolver typeResolver;
		MockDeviceInfo mockDeviceInfo;

		public static readonly string Foo = "Foo";

		class MockElementNode : IElementNode, IValueNode, IXmlLineInfo
		{
			public bool HasLineInfo() { return false; }

			public int LineNumber
			{
				get { return -1; }
			}

			public int LinePosition
			{
				get { return -1; }
			}

			public IXmlNamespaceResolver NamespaceResolver
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public object Value { get; set; }
			public Dictionary<XmlName, INode> Properties { get; set; }

			public List<XmlName> SkipProperties { get; set; }

			public NameScopeRef NameScopeRef => throw new NotImplementedException();

			public XmlType XmlType
			{
				get;
				set;
			}

			public string NamespaceURI
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public INode Parent
			{
				get
				{
					throw new NotImplementedException();
				}
				set { throw new NotImplementedException(); }
			}

			public List<INode> CollectionItems { get; set; }

			public void Accept(IXamlNodeVisitor visitor, INode parentNode)
			{
				throw new NotImplementedException();
			}

			public List<string> IgnorablePrefixes { get; set; }

			public INode Clone()
			{
				throw new NotImplementedException();
			}
		}

		// Constructor
		public override void Setup()
		{
			base.Setup();
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			var nsManager = new XmlNamespaceManager(new NameTable());
			nsManager.AddNamespace("local", "clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests");
			nsManager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2009/xaml");
			typeResolver = new Internals.XamlTypeResolver(nsManager, XamlParser.GetElementType, Assembly.GetCallingAssembly());
		}

		[Fact]
		public void BindingOnSelf()
		{
			var bindingString = "{Binding}";
			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});
			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal(Binding.SelfPath, ((Binding)binding).Path);
		}

		[Theory]
		[InlineData("{Binding {x:Static local:MarkupExpressionParserTests.Foo}}")]
		public void BindingWithImplicitPath(string bindingString)
		{
			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo", ((Binding)binding).Path);
		}

		[Fact]
		public void BindingWithPath()
		{
			var bindingString = "{Binding Path=Foo}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo", ((Binding)binding).Path);
		}

		[Fact]
		public void BindingWithComposedPath()
		{
			var bindingString = "{Binding Path=Foo.Bar}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo.Bar", ((Binding)binding).Path);
		}

		[Fact]
		public void BindingWithImplicitComposedPath()
		{
			var bindingString = "{Binding Path=Foo.Bar}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo.Bar", ((Binding)binding).Path);
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

		[Fact]
		public void BindingWithImplicitPathAndConverter()
		{
			var bindingString = "{Binding Foo, Converter={StaticResource Bar}}";
			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
				IProvideValueTarget = new MockValueProvider("Bar", new ReverseConverter()),
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo", ((Binding)binding).Path);
			Assert.NotNull(((Binding)binding).Converter);
			Assert.That(((Binding)binding).Converter, Is.InstanceOf<ReverseConverter>());
		}

		[Fact]
		public void BindingWithPathAndConverter()
		{
			var bindingString = "{Binding Path=Foo, Converter={StaticResource Bar}}";
			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
				IProvideValueTarget = new MockValueProvider("Bar", new ReverseConverter()),
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo", ((Binding)binding).Path);
			Assert.NotNull(((Binding)binding).Converter);
			Assert.That(((Binding)binding).Converter, Is.InstanceOf<ReverseConverter>());
		}

		[Fact]
		public void TestBindingMode()
		{
			var bindingString = "{Binding Foo, Mode=TwoWay}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo", ((Binding)binding).Path);
			Assert.Equal(BindingMode.TwoWay, ((Binding)binding).Mode);
		}

		[Fact]
		public void BindingStringFormat()
		{
			var bindingString = "{Binding Foo, StringFormat=Bar}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});
			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo", ((Binding)binding).Path);
			Assert.Equal("Bar", ((Binding)binding).StringFormat);
		}

		[Fact]
		public void BindingStringFormatWithEscapes()
		{
			var bindingString = "{Binding Foo, StringFormat='{}Hello {0}'}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo", ((Binding)binding).Path);
			Assert.Equal("Hello {0}", ((Binding)binding).StringFormat);
		}

		[Fact]
		public void BindingStringFormatWithoutEscaping()
		{
			var bindingString = "{Binding Foo, StringFormat='{0,20}'}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo", ((Binding)binding).Path);
			Assert.Equal("{0, 20}", ((Binding)binding).StringFormat);
		}

		[Fact]
		public void BindingStringFormatNumeric()
		{
			var bindingString = "{Binding Foo, StringFormat=P2}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo", ((Binding)binding).Path);
			Assert.Equal("P2", ((Binding)binding).StringFormat);
		}

		[Fact]
		public void BindingConverterParameter()
		{
			var bindingString = "{Binding Foo, ConverterParameter='Bar'}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo", ((Binding)binding).Path);
			Assert.Equal("Bar", ((Binding)binding).ConverterParameter);
		}

		[Fact]
		public void BindingsCompleteString()
		{
			var bindingString = "{Binding Path=Foo.Bar, StringFormat='{}Qux, {0}', Converter={StaticResource Baz}, Mode=OneWayToSource}";
			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
				IProvideValueTarget = new MockValueProvider("Baz", new ReverseConverter()),
			});

			Assert.That(binding, Is.InstanceOf<Binding>());
			Assert.Equal("Foo.Bar", ((Binding)binding).Path);
			Assert.NotNull(((Binding)binding).Converter);
			Assert.That(((Binding)binding).Converter, Is.InstanceOf<ReverseConverter>());
			Assert.Equal(BindingMode.OneWayToSource, ((Binding)binding).Mode);
			Assert.Equal("Qux, {0}", ((Binding)binding).StringFormat);
		}

		[Fact]
		public void BindingWithStaticConverter()
		{
			var bindingString = "{Binding Converter={x:Static local:ReverseConverter.Instance}}";

			var binding = (new MarkupExtensionParser()).ParseExpression(ref bindingString, new Internals.XamlServiceProvider(null, null)
			{
				IXamlTypeResolver = typeResolver,
			}) as Binding;

			Assert.NotNull(binding);
			Assert.Equal(".", binding.Path);
			Assert.That(binding.Converter, Is.TypeOf<ReverseConverter>());
		}

		public int FontSize { get; set; }

		[Theory]
		[InlineData("{OnPlatform Android=20, iOS=25}", "iOS", 25)]
		[Theory]
		[InlineData("{OnPlatform Android=20, Tizen=25}", "Tizen", 25)]
		[Theory]
		[InlineData("{OnPlatform Android=20, UWP=25}", "WinUI", 25)]
		[Theory]
		[InlineData("{OnPlatform Android=20, UWP=25}", "UWP", 25)]
		[Theory]
		[InlineData("{OnPlatform 20}", "iOS", 20)]
		[Theory]
		[InlineData("{OnPlatform 20}", "WinUI", 20)]
		[Theory]
		[InlineData("{OnPlatform 20}", "Foo", 20)]
		[Theory]
		[InlineData("{OnPlatform Android=23, Default=20}", "Foo", 20)]
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

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("{OnIdiom Phone=23, Tablet=25, Default=20}", "Tablet", 25)]
		[Theory]
		[InlineData("{OnIdiom Phone=23, Tablet=25, Desktop=26, TV=30, Watch=10}", "Desktop", 26)]
		[Theory]
		[InlineData("{OnIdiom Phone=23, Tablet=25, Desktop=26, TV=30, Watch=10}", "Watch", 10)]
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

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("{Binding 'Foo}")]
		[Theory]
		[InlineData("{Binding Foo, Converter={StaticResource Bar}?}")]
		public void InvalidExpressions(string expression)
		{
			var serviceProvider = new Internals.XamlServiceProvider(null, null);
			serviceProvider.IXamlTypeResolver = typeResolver;
			serviceProvider.IProvideValueTarget = new MockValueProvider("Bar", new ReverseConverter());
			Assert.Throws<XamlParseException>(() => (new MarkupExtensionParser()).ParseExpression(ref expression, serviceProvider));
		}
	}
}
