using System;
using NUnit.Framework;

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

	void OnTextChanged(object sender, EventArgs e) => Assert.Pass();

	[TestFixture]
	class Tests
	{
		[Test]
		public void NonGenericEventHanlders([Values] XamlInflator inflator)
		{
			var layout = new Gh4130(inflator);
			var control = layout.Content as Gh4130Control;
			control.FireEvent();
			Assert.Fail();
		}
	}
}
