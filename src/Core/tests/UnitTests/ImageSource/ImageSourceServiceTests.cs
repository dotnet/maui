// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.ImageSource
{
	[Category(TestCategory.Core, TestCategory.ImageSource)]
	public class ImageSourceServiceTests
	{
		[Fact]
		public void ResolvesToConcreteTypeOverInterface()
		{
			var provider = CreateImageSourceServiceProvider(services =>
			{
				services.AddService<IStreamImageSource, StreamImageSourceService>();
				services.AddService<MultipleInterfacesImageSourceStub, UriImageSourceService>();
			});

			var service = provider.GetRequiredImageSourceService(new MultipleInterfacesImageSourceStub());
			Assert.IsType<UriImageSourceService>(service);
		}

		private IImageSourceServiceProvider CreateImageSourceServiceProvider(Action<IImageSourceServiceCollection> configure)
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureImageSources(configure)
				.Build();

			var provider = mauiApp.Services.GetRequiredService<IImageSourceServiceProvider>();

			return provider;
		}
	}
}
