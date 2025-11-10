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
	public class Bz36422Control : ContentView
	{
		public IList<ContentView> Views { get; set; }
	}

	public partial class Bz36422 : ContentPage
	{
		public Bz36422()
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
			public void xArrayCanBeAssignedToIListT(XamlInflator inflator)
			{
				var layout = new Bz36422(inflator);
				Assert.Equal(3, layout.control.Views.Count);
			}
		}
	}
}