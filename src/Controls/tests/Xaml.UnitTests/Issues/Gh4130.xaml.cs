using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4130Control : ContentView
{
	public delegate void TextChangedEventHandler(object sender, TextChangedEventArgs args);
#pragma warning disable 067
	public event TextChangedEventHandler TextChanged;
#pragma warning restore 067
	public void FireEvent() => TextChanged?.Invoke(this, new TextChangedEventArgs(null, null));
}

public partial class Gh4130 : ContentPage
{
	public bool EventFired { get; set; }

	public Gh4130()
	{
		InitializeComponent();
		var c = new Gh4130Control();
	}

	void OnTextChanged(object sender, EventArgs e)
	{
		EventFired = true;
	}


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
		public void NonGenericEventHanlders(XamlInflator inflator)
		{
			var layout = new Gh4130(inflator);
			var control = layout.Content as Gh4130Control;
			control.FireEvent();
			Assert.True(layout.EventFired, "Event handler should have been called");
		}
	}
}
