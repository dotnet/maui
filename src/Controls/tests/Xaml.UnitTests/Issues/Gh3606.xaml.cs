using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// related to https://github.com/dotnet/maui/issues/23711
public partial class Gh3606 : ContentPage
{
	public Gh3606() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void BindingsWithSourceAndInvalidPathAreNotCompiled([Values] XamlInflator inflator)
		{
			var view = new Gh3606(inflator);

			var binding = view.Label.GetContext(Label.TextProperty).Bindings.GetValue();
			Assert.That(binding, Is.TypeOf<Binding>());
		}
	}
}
