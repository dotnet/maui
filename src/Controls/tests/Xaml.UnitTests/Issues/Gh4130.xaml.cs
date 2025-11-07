using System;
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
	public Gh4130()
	{
		InitializeComponent();
		var c = new Gh4130Control();
	}

	void OnTextChanged(object sender, EventArgs e)
	{
		// TODO: XUnit has no // TODO: XUnit has no Assert.Pass() - test passes if no exception is thrown - test passes if no exception is thrown
	}


	public class Tests
	{
		[Theory]
		[Values]
		public void NonGenericEventHanlders(XamlInflator inflator)
		{
			var layout = new Gh4130(inflator);
			var control = layout.Content as Gh4130Control;
			control.FireEvent();
			Assert.Fail();
		}
	}
}
