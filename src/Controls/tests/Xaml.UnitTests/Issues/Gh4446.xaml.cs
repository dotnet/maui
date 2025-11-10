using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4446Item
{
	public string Id { get; set; }
	public string Text { get; set; }
	public string Description { get; set; }
}

public partial class Gh4446 : ContentPage
{
	public Gh4446() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
		[Theory]
		[Values]
		public void BindingThrowsOnWrongConverterParameter(XamlInflator inflator)
		{
			Assert.Null(Record.Exception(() => new Gh4446(inflator) { BindingContext = new Gh4446Item { Text = null } }));
		}
	}
}
