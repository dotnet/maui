using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XamlInflatorSwitch : ContentPage
{
	public XamlInflatorSwitch() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void TestRuntimeInflator() => XamlInflatorTestsHelpers.TestInflator(typeof(XamlInflatorSwitch), XamlInflator.Runtime, true);

		[Test]
		public void TestInflation()
		{
			var page = new XamlInflatorSwitch();
			Assert.That(page.label.Text, Is.EqualTo("Welcome to .NET MAUI!"), "Label text should be 'Welcome to .NET MAUI!'");
		}
	}
}