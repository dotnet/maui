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
		[Theory]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(true, true)]
		public void ResolvesImageSourceServiceInTypicalScenarios(bool registerInterfaces, bool registerClasses)
		{
			var provider = CreateImageSourceServiceProvider(services =>
			{
				if (registerInterfaces)
				{
					services.AddService<IStreamImageSource, StreamImageSourceService>();
					services.AddService<IFileImageSource, FileImageSourceService>();
				}

				if (registerClasses)
				{
					services.AddService<StreamImageSourceStub, StreamImageSourceService>();
					services.AddService<FileImageSourceStub, FileImageSourceService>();
				}
			});

			var service = provider.GetRequiredImageSourceService(new FileImageSourceStub());

			Assert.IsType<FileImageSourceService>(service);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void ResolvesCorrectServiceInstance(bool registerInterfaces)
		{
			var serviceForFirst = new CombinedImageSourceService();
			var serviceForSecond = new CombinedImageSourceService();
			
			var provider = CreateImageSourceServiceProvider(services =>
			{
				if (registerInterfaces)
				{
					services.AddService<IFirstImageSource>(_ => serviceForFirst);
					services.AddService<ISecondImageSource>(_ => serviceForSecond);
				}
				else
				{
					services.AddService<FirstImageSource>(_ => serviceForFirst);
					services.AddService<SecondImageSource>(_ => serviceForSecond);
				}
			});

			var actualFirstService = provider.GetRequiredImageSourceService(new FirstImageSource());
			var actualSecondService = provider.GetRequiredImageSourceService(new SecondImageSource());

			Assert.Same(serviceForFirst, actualFirstService);
			Assert.Same(serviceForSecond, actualSecondService);
		}

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

		[Fact]
		public void GetsImageSourceTypeEvenWhenNoServiceWasRegistered()
		{
			var provider = CreateImageSourceServiceProvider(services => {});

			var imageSource = provider.GetImageSourceType(typeof(StreamImageSourceStub));

			Assert.Equal(typeof(IStreamImageSource), imageSource);
		}

		[Fact]
		public void GetsImageSourceServiceTypeEvenWhenNoServiceWasRegistered()
		{
			var provider = CreateImageSourceServiceProvider(services => {});

			var service = provider.GetImageSourceServiceType(typeof(IStreamImageSource));

			Assert.Equal(typeof(IImageSourceService<IStreamImageSource>), service);
		}

		[Fact]
		public void GetsImageSourceTypeForRegisteredService()
		{
			var provider = CreateImageSourceServiceProvider(services =>
			{
				services.AddService<StreamImageSourceStub, StreamImageSourceService>();
			});

			var service = provider.GetImageSourceType(typeof(StreamImageSourceStub));

			Assert.Equal(typeof(IStreamImageSource), service);
		}

		[Fact]
		public void GetsImageSourceServiceTypeForRegisteredService()
		{
			var provider = CreateImageSourceServiceProvider(services =>
			{
				services.AddService<StreamImageSourceStub, StreamImageSourceService>();
			});

			var service = provider.GetImageSourceServiceType(typeof(StreamImageSourceStub));

			Assert.Equal(typeof(IImageSourceService<StreamImageSourceStub>), service);
		}

		private IImageSourceServiceProvider CreateImageSourceServiceProvider(Action<IImageSourceServiceCollection> configure)
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureImageSources(configure)
				.Build();

			var provider = mauiApp.Services.GetRequiredService<IImageSourceServiceProvider>();

			return provider;
		}

		private interface IFirstImageSource : IImageSource { }
		private interface ISecondImageSource : IImageSource { }
		private class FirstImageSource : IFirstImageSource { public bool IsEmpty => throw new NotImplementedException(); }
		private class SecondImageSource : ISecondImageSource { public bool IsEmpty => throw new NotImplementedException(); }
		private class CombinedImageSourceService : IImageSourceService<IFirstImageSource>, IImageSourceService<ISecondImageSource> { }
	}
}
