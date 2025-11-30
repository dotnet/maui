using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui32924 : ContentPage
{
	public Maui32924()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			Application.SetCurrentApplication(null!);
			DispatcherProvider.SetCurrent(null!);
		}

		[Test]
		public void ExplicitFindAncestorBindingContextMode([Values] XamlInflator inflator)
		{
			// Test with explicit Mode=FindAncestorBindingContext (issue #32924)
			var page = new Maui32924(inflator);
			Assert.That(page.ExplicitModeLabel.Text, Is.EqualTo("TestName"));
		}

		[Test]
		public void ImplicitFindAncestorBindingContextMode([Values] XamlInflator inflator)
		{
			// Test with implicit mode (non-Element type should infer FindAncestorBindingContext)
			var page = new Maui32924(inflator);
			Assert.That(page.ImplicitModeLabel.Text, Is.EqualTo("TestName"));
		}

		[Test]
		public void FindAncestorMode([Values] XamlInflator inflator)
		{
			// Test with FindAncestor mode (Element type)
			var page = new Maui32924(inflator);
			Assert.That(page.FindAncestorLabel.Text, Is.EqualTo("Stack1"));
		}

		[Test]
		public void SelfMode([Values] XamlInflator inflator)
		{
			// Test with Self mode
			var page = new Maui32924(inflator);
			Assert.That(page.SelfLabel.Text, Is.EqualTo("SelfTest"));
		}
	}
}

public class Maui32924ViewModel
{
	public string Name => "TestName";
}
