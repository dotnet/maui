using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2578 : ContentPage
{
	public Issue2578() => InitializeComponent();


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
		[Theory(Skip = "[Bug] NamedSizes don't work in triggers: https://github.com/xamarin/Microsoft.Maui.Controls/issues/13831")]
		[Values]
		public void MultipleTriggers(XamlInflator inflator)
		{
			Issue2578 layout = new Issue2578(inflator);

			Assert.Null(layout.label.Text);
			Assert.Null(layout.label.BackgroundColor);
			Assert.Equal(Colors.Olive, layout.label.TextColor);
			layout.label.Text = "Foo";
			Assert.Equal(Colors.Red, layout.label.BackgroundColor);
		}
	}
}