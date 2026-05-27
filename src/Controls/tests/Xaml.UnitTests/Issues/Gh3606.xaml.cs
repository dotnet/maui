using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// related to https://github.com/dotnet/maui/issues/23711
public partial class Gh3606 : ContentPage
{
	public Gh3606() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void BindingsWithSourceAndInvalidPathAreNotCompiled(XamlInflator inflator)
		{
			var view = new Gh3606(inflator);

			var binding = view.Label.GetContext(Label.TextProperty).Bindings.GetValue();
			Assert.IsType<Binding>(binding);
		}
	}
}
