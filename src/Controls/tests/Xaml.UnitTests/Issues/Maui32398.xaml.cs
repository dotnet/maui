using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui32398 : ContentPage
{
	public Maui32398() => InitializeComponent();
	
	public readonly BindableProperty NonStaticProperty =
		BindableProperty.Create(nameof(NonStatic), typeof(string), typeof(Maui32398), default(string));
	public string NonStatic
	{
		get;set;
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void NonStaticBP(XamlInflator inflator)
        {
			var page = new Maui32398(inflator);
			Assert.Equal("foo", page.NonStatic);
			Assert.NotEqual("foo", page.GetValue(page.NonStaticProperty));
        }
	}
}