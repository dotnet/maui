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
	[AcceptEmptyServiceProvider]
	public class FooExtension : IMarkupExtension<IServiceProvider>
	{
		public IServiceProvider ProvideValue(IServiceProvider serviceProvider)
		{
			return serviceProvider;
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<IServiceProvider>).ProvideValue(serviceProvider);
		}
	}

	public partial class AcceptEmptyServiceProvider : ContentPage
	{
		public AcceptEmptyServiceProvider()
		{
			InitializeComponent();
		}

		public IServiceProvider ServiceProvider { get; set; }


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
			public void ServiceProviderIsNullOnAttributedExtensions(XamlInflator inflator)
			{
				var p = new AcceptEmptyServiceProvider(inflator);
				Assert.Null(p.ServiceProvider);
			}
		}
	}
}
