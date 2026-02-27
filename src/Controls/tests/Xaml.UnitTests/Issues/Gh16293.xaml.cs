using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh16293 : ContentPage
{
	public Gh16293() => InitializeComponent();


	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ShouldResolveNested(XamlInflator inflator)
		{
			var layout = new Gh16293(inflator);
			Assert.Equal("LibraryConstant", layout.Label1.Text);
			Assert.Equal("NestedLibraryConstant", layout.Label2.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void ShouldResolveInternalNested(XamlInflator inflator)
		{
			var layout = new Gh16293(inflator);
			Assert.Equal("InternalLibraryConstant", layout.Label3.Text);
			Assert.Equal("InternalNestedLibraryConstant", layout.Label4.Text);
		}
	}
}
