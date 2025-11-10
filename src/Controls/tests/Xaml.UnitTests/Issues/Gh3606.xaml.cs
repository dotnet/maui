using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// related to https://github.com/dotnet/maui/issues/23711
public partial class Gh3606 : ContentPage
{
	public Gh3606() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}


		[Theory]
		[Values]
		public void BindingsWithSourceAndInvalidPathAreNotCompiled(XamlInflator inflator)
		{
			var view = new Gh3606(inflator);

			var binding = view.Label.GetContext(Label.TextProperty).Bindings.GetValue();
			Assert.IsType<Binding>(binding);
		}
	}
}
