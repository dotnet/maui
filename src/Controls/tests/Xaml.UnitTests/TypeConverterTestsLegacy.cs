using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class FooConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => true;
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return new Foo { Value = (string)value };
		}
	}

	public class BarConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => true;

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return new Bar { Value = (string)value };
		}
	}

	public class QuxConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => true;

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return new Qux { Value = (string)value };
		}
	}

	public class FooBarConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => true;

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return new FooBar { Value = (string)value };
		}
	}

	public class Foo
	{
		public string Value { get; set; }
	}

	public class Bar
	{
		public string Value { get; set; }
	}

	public class Baz
	{
		public string Value { get; set; }
	}

	public class Qux
	{
		public string Value { get; set; }
	}

	[System.ComponentModel.TypeConverter(typeof(FooBarConverter))]
	public class FooBar
	{
		public string Value { get; set; }
	}

	public class Bindable : BindableObject
	{
		[System.ComponentModel.TypeConverter(typeof(FooConverter))]
		public Foo Foo { get; set; }

		public static readonly BindableProperty BarProperty = BindableProperty.Create(nameof(Bar), typeof(Bar), typeof(Bindable), default(Bar));

		[System.ComponentModel.TypeConverter(typeof(BarConverter))]
		public Bar Bar
		{
			get { return (Bar)GetValue(BarProperty); }
			set { SetValue(BarProperty, value); }
		}

		public Baz Baz { get; set; }

		public static readonly BindableProperty QuxProperty = BindableProperty.CreateAttached(nameof(Qux), typeof(Qux), typeof(Bindable), default(Qux));

		[System.ComponentModel.TypeConverter(typeof(QuxConverter))]
		public static Qux GetQux(BindableObject bindable)
		{
			return (Qux)bindable.GetValue(QuxProperty);
		}

		public static void SetQux(BindableObject bindable, Qux value)
		{
			bindable.SetValue(QuxProperty, value);
		}

		public FooBar FooBar { get; set; }
	}

	internal class MockNameSpaceResolver : IXmlNamespaceResolver
	{
		public System.Collections.Generic.IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
		{
			throw new NotImplementedException();
		}

		public string LookupNamespace(string prefix)
		{
			return "";
		}

		public string LookupPrefix(string namespaceName)
		{
			return "";
		}
	}

	[Collection("Xaml Inflation")]
	public class TypeConverterTestsLegacy : BaseTestFixture
	{
		[Fact]
		public void TestSetPropertyWithoutConverter()
		{
			var baz = new Baz();
			var node = new ValueNode(baz, new MockNameSpaceResolver());
			var bindable = new Bindable();

			Assert.Null(bindable.Baz);
			var rootNode = new XamlLoader.RuntimeRootNode(new XmlType("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable", null), bindable, null)
			{
				Properties = {
					{ new XmlName (null, "Baz"), node },
				}
			};
			var context = new HydrationContext { RootElement = new Label() };
			rootNode.Accept(new CreateValuesVisitor(context), null);
			node.Accept(new ApplyPropertiesVisitor(context), rootNode);
			Assert.Equal(baz, bindable.Baz);

		}

		[Fact]
		public void TestFailOnMissingOrWrongConverter()
		{
			var node = new ValueNode("baz", new MockNameSpaceResolver());
			var bindable = new Bindable();

			Assert.Null(bindable.Baz);
			var rootNode = new XamlLoader.RuntimeRootNode(new XmlType("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable", null), bindable, null)
			{
				Properties = {
					{ new XmlName (null, "Baz"), node },
				}
			};
			var context = new HydrationContext { RootElement = new Label() };
			rootNode.Accept(new CreateValuesVisitor(context), null);
			Assert.Throws<XamlParseException>(() => node.Accept(new ApplyPropertiesVisitor(context), rootNode));
		}

		[Fact]
		public void TestConvertNonBindableProperty()
		{
			var node = new ValueNode("foo", new MockNameSpaceResolver());
			var bindable = new Bindable();

			Assert.Null(bindable.Foo);
			var rootNode = new XamlLoader.RuntimeRootNode(new XmlType("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable", null), bindable, null)
			{
				Properties = {
					{ new XmlName (null, "Foo"), node },
				}
			};

			var context = new HydrationContext { RootElement = new Label() };
			rootNode.Accept(new CreateValuesVisitor(context), null);
			node.Accept(new ApplyPropertiesVisitor(context), rootNode);
			Assert.NotNull(bindable.Foo);
			Assert.IsType<Foo>(bindable.Foo);
			Assert.Equal("foo", bindable.Foo.Value);
		}

		[Fact]
		public void TestConvertBindableProperty()
		{
			var node = new ValueNode("bar", new MockNameSpaceResolver());
			var bindable = new Bindable();

			Assert.Null(bindable.Bar);
			var rootNode = new XamlLoader.RuntimeRootNode(new XmlType("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable", null), bindable, null)
			{
				Properties = {
					{ new XmlName (null, "Bar"), node },
				}
			};
			var context = new HydrationContext { RootElement = new Label() };
			rootNode.Accept(new CreateValuesVisitor(context), null);
			node.Accept(new ApplyPropertiesVisitor(context), rootNode);
			Assert.NotNull(bindable.Bar);
			Assert.IsType<Bar>(bindable.Bar);
			Assert.Equal("bar", bindable.Bar.Value);
		}

		[Fact]
		public void TestConvertAttachedBindableProperty()
		{
			var node = new ValueNode("qux", new MockNameSpaceResolver());
			var bindable = new Bindable();

			Assert.Null(Bindable.GetQux(bindable));
			var rootNode = new XamlLoader.RuntimeRootNode(new XmlType("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable", null), bindable, null)
			{
				Properties = {
					{ new XmlName ("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable.Qux"), node },
				}
			};
			var context = new HydrationContext { RootElement = new Label() };
			rootNode.Accept(new CreateValuesVisitor(context), null);
			node.Accept(new ApplyPropertiesVisitor(context), rootNode);
			Assert.NotNull(Bindable.GetQux(bindable));
			Assert.IsType<Qux>(Bindable.GetQux(bindable));
			Assert.Equal("qux", Bindable.GetQux(bindable).Value);
		}

		[Fact]
		public void TestConvertWithAttributeOnType()
		{
			var node = new ValueNode("foobar", new MockNameSpaceResolver());
			var bindable = new Bindable();

			Assert.Null(bindable.FooBar);
			var rootNode = new XamlLoader.RuntimeRootNode(new XmlType("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable", null), bindable, null)
			{
				Properties = {
					{ new XmlName (null, "FooBar"), node },
				}
			};
			var context = new HydrationContext { RootElement = new Label() };
			rootNode.Accept(new CreateValuesVisitor(context), null);
			node.Accept(new ApplyPropertiesVisitor(context), rootNode);

			Assert.NotNull(bindable.FooBar);
			Assert.IsType<FooBar>(bindable.FooBar);
			Assert.Equal("foobar", bindable.FooBar.Value);
		}


#if !WINDOWS_PHONE
		[Theory]
		[InlineData("en-GB")]
		[InlineData("fr-FR")]
		public void TestCultureOnThickness(string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

			var xaml = @"<Page Padding=""1.1, 2""/>";
			var page = new Page().LoadFromXaml(xaml);
			Assert.Equal(new Thickness(1.1, 2), page.Padding);
		}
#endif
	}
}