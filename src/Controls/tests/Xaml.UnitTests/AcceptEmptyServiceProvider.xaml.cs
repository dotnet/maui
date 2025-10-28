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

		public AcceptEmptyServiceProvider(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public IServiceProvider ServiceProvider { get; set; }		class Tests
		{
			[InlineData(true)]
			[InlineData(false)]
			public void ServiceProviderIsNullOnAttributedExtensions(bool useCompiledXaml)
			{
				var p = new AcceptEmptyServiceProvider(useCompiledXaml);
				Assert.Null(p.ServiceProvider);
			}
		}
	}
}
