using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz30684 : ContentPage
	{
		public Bz30684()
		{
			InitializeComponent();
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
			public void XReferenceFindObjectsInParentNamescopes(XamlInflator inflator)
			{
				var layout = new Bz30684(inflator);
				var cell = (TextCell)layout.listView.TemplatedItems.GetOrCreateContent(0, null);
				Assert.Equal("Foo", cell.Text);
			}
		}
	}
}