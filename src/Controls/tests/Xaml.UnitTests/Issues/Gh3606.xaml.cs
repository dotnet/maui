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
		internal void BindingsWithXReferenceSourceResolveAgainstReferencedType(XamlInflator inflator)
		{
			// Source={x:Reference page} points to ContentPage, which has a Content property.
			// The binding path "Content" resolves against ContentPage, so the source generator
			// compiles it instead of falling back to runtime Binding.
			var view = new Gh3606(inflator);

			var binding = view.Label.GetContext(Label.TextProperty).Bindings.GetValue();
			Assert.IsAssignableFrom<BindingBase>(binding);
		}
	}
}
