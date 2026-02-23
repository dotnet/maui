using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz30684 : ContentPage
{
	public Bz30684()
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
		internal void XReferenceFindObjectsInParentNamescopes(XamlInflator inflator)
		{
			var layout = new Bz30684(inflator);
			var cell = (TextCell)layout.listView.TemplatedItems.GetOrCreateContent(0, null);
			Assert.Equal("Foo", cell.Text);
		}
	}
}