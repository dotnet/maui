using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui24384 : ContentPage
{
	public static System.Collections.Immutable.ImmutableArray<string> StaticLetters => ["A", "B", "C"];

	public Maui24384() => InitializeComponent();

	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			Application.SetCurrentApplication(null);
			DispatcherProvider.SetCurrent(null);
			Application.Current = null;
		}

		[Theory]
		[Values]
		public void ImmutableToIList(XamlInflator inflator)
		{
			var page = new Maui24384(inflator);
			var picker = page.Content as Picker;
			Assert.Equivalent(Maui24384.StaticLetters, picker.ItemsSource);
		}
	}
}
