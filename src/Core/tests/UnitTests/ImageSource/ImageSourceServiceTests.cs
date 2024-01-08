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
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddKeyedSingleton<IImageSourceService, StreamImageSourceService>(typeof(IStreamImageSource));
			builder.Services.AddKeyedSingleton<IImageSourceService, UriImageSourceService>(typeof(MultipleInterfacesImageSourceStub));
			var provider = (IKeyedServiceProvider)builder.Build().Services;

			var service = provider.GetRequiredKeyedService(typeof(MultipleInterfacesImageSourceStub));
			Assert.IsType<UriImageSourceService>(service);
		}
	}
}
