using System;
using System.Globalization;
using System.Xml;
using NUnit.Framework;
using Microsoft.Maui.Controls.Core.UnitTests;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class FooConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			return new Foo { Value = value };
		}
	}

	public class BarConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			return new Bar { Value = (string)value };
		}
	}

	public class QuxConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			return new Qux { Value = (string)value };
		}
	}

	public class FooBarConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
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

	[TypeConverter(typeof(FooBarConverter))]
	public class FooBar
	{
		public string Value { get; set; }
	}

	public class Bindable : BindableObject
	{
		[TypeConverter(typeof(FooConverter))]
		public Foo Foo { get; set; }

		public static readonly BindableProperty BarProperty = BindableProperty.Create(nameof(Bar), typeof(Bar), typeof(Bindable), default(Bar));

		[TypeConverter(typeof(BarConverter))]
		public Bar Bar
		{
			get { return (Bar)GetValue(BarProperty); }
			set { SetValue(BarProperty, value); }
		}

		public Baz Baz { get; set; }

		public static readonly BindableProperty QuxProperty = BindableProperty.CreateAttached(nameof(Qux), typeof(Qux), typeof(Bindable), default(Qux));

		[TypeConverter(typeof(QuxConverter))]
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

	[TestFixture]
	public class TypeConverterTestsLegacy : BaseTestFixture
	{
		[Test]
		public void TestSetPropertyWithoutConverter()
		{
			var baz = new Baz();
			var node = new ValueNode(baz, new MockNameSpaceResolver());
			var bindable = new Bindable();

			Assert.IsNull(bindable.Baz);
			var rootNode = new XamlLoader.RuntimeRootNode(new XmlType("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable", null), bindable, null)
			{
				Properties = {
					{ new XmlName (null, "Baz"), node },
				}
			};
			var context = new HydrationContext { RootElement = new Label() };
			rootNode.Accept(new CreateValuesVisitor(context), null);
			node.Accept(new ApplyPropertiesVisitor(context), rootNode);
			Assert.AreEqual(baz, bindable.Baz);

		}

		[Test]
		public void TestFailOnMissingOrWrongConverter()
		{
			var node = new ValueNode("baz", new MockNameSpaceResolver());
			var bindable = new Bindable();

			Assert.IsNull(bindable.Baz);
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

		[Test]
		public void TestConvertNonBindableProperty()
		{
			var node = new ValueNode("foo", new MockNameSpaceResolver());
			var bindable = new Bindable();

			Assert.IsNull(bindable.Foo);
			var rootNode = new XamlLoader.RuntimeRootNode(new XmlType("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable", null), bindable, null)
			{
				Properties = {
					{ new XmlName (null, "Foo"), node },
				}
			};

			var context = new HydrationContext { RootElement = new Label() };
			rootNode.Accept(new CreateValuesVisitor(context), null);
			node.Accept(new ApplyPropertiesVisitor(context), rootNode);
			Assert.IsNotNull(bindable.Foo);
			Assert.That(bindable.Foo, Is.TypeOf<Foo>());
			Assert.AreEqual("foo", bindable.Foo.Value);
		}

		[Test]
		public void TestConvertBindableProperty()
		{
			var node = new ValueNode("bar", new MockNameSpaceResolver());
			var bindable = new Bindable();

			Assert.IsNull(bindable.Bar);
			var rootNode = new XamlLoader.RuntimeRootNode(new XmlType("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable", null), bindable, null)
			{
				Properties = {
					{ new XmlName (null, "Bar"), node },
				}
			};
			var context = new HydrationContext { RootElement = new Label() };
			rootNode.Accept(new CreateValuesVisitor(context), null);
			node.Accept(new ApplyPropertiesVisitor(context), rootNode);
			Assert.IsNotNull(bindable.Bar);
			Assert.That(bindable.Bar, Is.TypeOf<Bar>());
			Assert.AreEqual("bar", bindable.Bar.Value);
		}

		[Test]
		public void TestConvertAttachedBindableProperty()
		{
			var node = new ValueNode("qux", new MockNameSpaceResolver());
			var bindable = new Bindable();

			Assert.IsNull(Bindable.GetQux(bindable));
			var rootNode = new XamlLoader.RuntimeRootNode(new XmlType("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable", null), bindable, null)
			{
				Properties = {
					{ new XmlName ("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable.Qux"), node },
				}
			};
			var context = new HydrationContext { RootElement = new Label() };
			rootNode.Accept(new CreateValuesVisitor(context), null);
			node.Accept(new ApplyPropertiesVisitor(context), rootNode);
			Assert.IsNotNull(Bindable.GetQux(bindable));
			Assert.That(Bindable.GetQux(bindable), Is.TypeOf<Qux>());
			Assert.AreEqual("qux", Bindable.GetQux(bindable).Value);
		}

		[Test]
		public void TestConvertWithAttributeOnType()
		{
			var node = new ValueNode("foobar", new MockNameSpaceResolver());
			var bindable = new Bindable();

			Assert.IsNull(bindable.FooBar);
			var rootNode = new XamlLoader.RuntimeRootNode(new XmlType("clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", "Bindable", null), bindable, null)
			{
				Properties = {
					{ new XmlName (null, "FooBar"), node },
				}
			};
			var context = new HydrationContext { RootElement = new Label() };
			rootNode.Accept(new CreateValuesVisitor(context), null);
			node.Accept(new ApplyPropertiesVisitor(context), rootNode);

			Assert.IsNotNull(bindable.FooBar);
			Assert.That(bindable.FooBar, Is.TypeOf<FooBar>());
			Assert.AreEqual("foobar", bindable.FooBar.Value);
		}


#if !WINDOWS_PHONE
		[TestCase("en-GB"), TestCase("fr-FR")]
		public void TestCultureOnThickness(string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

			var xaml = @"<Page Padding=""1.1, 2""/>";
			var page = new Page().LoadFromXaml(xaml);
			Assert.AreEqual(new Thickness(1.1, 2), page.Padding);
		}
#endif
	}
}