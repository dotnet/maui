using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh16293 : ContentPage
{
	public Gh16293() => InitializeComponent();


	[TestFixture]
	class Tests
	{
		[Test]
		public void ShouldResolveNested([Values] XamlInflator inflator)
		{
			var layout = new Gh16293(inflator);
			Assert.That(layout.Label1.Text, Is.EqualTo("LibraryConstant"));
			Assert.That(layout.Label2.Text, Is.EqualTo("NestedLibraryConstant"));
		}

		[Test]
		public void ShouldResolveInternalNested([Values] XamlInflator inflator)
		{
			var layout = new Gh16293(inflator);
			Assert.That(layout.Label3.Text, Is.EqualTo("InternalLibraryConstant"));
			Assert.That(layout.Label4.Text, Is.EqualTo("InternalNestedLibraryConstant"));
		}
	}
}
