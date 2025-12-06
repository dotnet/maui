using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui32924 : ContentPage
{
	public Maui32924()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ExplicitFindAncestorBindingContextMode(XamlInflator inflator)
		{
			// Test with explicit Mode=FindAncestorBindingContext (issue #32924)
			var page = new Maui32924(inflator);
			Assert.Equal("TestName", page.ExplicitModeLabel.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void ImplicitFindAncestorBindingContextMode(XamlInflator inflator)
		{
			// Test with implicit mode (non-Element type should infer FindAncestorBindingContext)
			var page = new Maui32924(inflator);
			Assert.Equal("TestName", page.ImplicitModeLabel.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void FindAncestorMode(XamlInflator inflator)
		{
			// Test with FindAncestor mode (Element type)
			var page = new Maui32924(inflator);
			Assert.Equal("Stack1", page.FindAncestorLabel.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void SelfMode(XamlInflator inflator)
		{
			// Test with Self mode
			var page = new Maui32924(inflator);
			Assert.Equal("SelfTest", page.SelfLabel.Text);
		}
	}
}

public class Maui32924ViewModel
{
	public string Name => "TestName";
}
