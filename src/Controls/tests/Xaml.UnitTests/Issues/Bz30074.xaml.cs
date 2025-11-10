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
	public partial class Bz30074 : ContentPage
	{
		public Bz30074()
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
			public void DataTriggerInTemplates(XamlInflator inflator)
			{
				var layout = new Bz30074(inflator);
				Assert.Null(layout.image.Source);

				layout.BindingContext = new { IsSelected = true };
				Assert.Equal("Add.png", ((FileImageSource)layout.image.Source).File);

				layout.BindingContext = new { IsSelected = false };
				Assert.Null(layout.image.Source);
			}
		}
	}
}

