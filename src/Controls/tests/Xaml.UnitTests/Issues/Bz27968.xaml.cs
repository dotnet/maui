using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz27968Page : ContentPage
{
}

public partial class Bz27968 : Bz27968Page
{
	public Bz27968()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void BaseClassIdentifiersAreValidForResources(XamlInflator inflator)
		{
			var layout = new Bz27968(inflator);
			Assert.IsType<ListView>(layout.Resources["listView"]);
		}
	}
}
