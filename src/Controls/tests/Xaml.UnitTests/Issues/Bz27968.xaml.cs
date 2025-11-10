using System;
using Microsoft.Maui.ApplicationModel;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz27968Page : ContentPage
	{
	}

	public partial class Bz27968 : Bz27968Page
	{
		public Bz27968()
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
			public void BaseClassIdentifiersAreValidForResources(XamlInflator inflator)
			{
				var layout = new Bz27968(inflator);
				Assert.IsType<ListView>(layout.Resources["listView"]);
			}
		}
	}
}
