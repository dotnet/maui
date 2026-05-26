using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
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

		[Collection("Xaml Inflation")]
		public class Tests
		{
			[Theory]
			[XamlInflatorData]
			internal void ServiceProviderIsNullOnAttributedExtensions(XamlInflator inflator)
			{
				var p = new AcceptEmptyServiceProvider(inflator);
				Assert.Null(p.ServiceProvider);
			}
		}
	}
}
