namespace Microsoft.Maui.Controls.Xaml.UnitTests;

using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

public partial class Maui29334 : ContentPage
{

	public Maui29334() => InitializeComponent();

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
		internal void OnIdiomGridLength(XamlInflator inflator)
		{
			var page = new Maui29334(inflator);
			
		}
	}
}