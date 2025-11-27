using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class BaseClassForStaticTest
{
	public const string BaseClassString = "base class string";
	public static string BaseStaticProperty { get; } = "base static property";
	public static string BaseStaticField = "base static field";
}

public class DerivedClassForStaticTest : BaseClassForStaticTest
{
	public const string DerivedClassString = "derived class string";
}

public partial class XStaticInheritance : ContentPage
{
	public XStaticInheritance() => InitializeComponent();

	public XStaticInheritance(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void XStaticCanAccessInheritedConstant([Values] XamlInflator inflator)
		{
			var layout = new XStaticInheritance(inflator);
			Assert.That(layout.baseConstant.Text, Is.EqualTo("base class string"));
		}

		[Test]
		public void XStaticCanAccessInheritedStaticProperty([Values] XamlInflator inflator)
		{
			var layout = new XStaticInheritance(inflator);
			Assert.That(layout.baseProperty.Text, Is.EqualTo("base static property"));
		}

		[Test]
		public void XStaticCanAccessInheritedStaticField([Values] XamlInflator inflator)
		{
			var layout = new XStaticInheritance(inflator);
			Assert.That(layout.baseField.Text, Is.EqualTo("base static field"));
		}

		[Test]
		public void XStaticCanAccessDerivedClassMember([Values] XamlInflator inflator)
		{
			var layout = new XStaticInheritance(inflator);
			Assert.That(layout.derivedConstant.Text, Is.EqualTo("derived class string"));
		}
	}
}
