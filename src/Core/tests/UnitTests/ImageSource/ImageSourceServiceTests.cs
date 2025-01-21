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
					services.AddService<IFirstImageSource, FirstImageSourceService>();
					services.AddService<ISecondImageSource, SecondImageSourceService>();
				}

				if (registerClasses)
				{
					services.AddService<FirstImageSource, FirstImageSourceService>();
					services.AddService<SecondImageSource, SecondImageSourceService>();
				}
			});

			var service = provider.GetRequiredImageSourceService(new FirstImageSource());

			Assert.IsType<FirstImageSourceService>(service);
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
		public void ResolvesToConcreteTypeOverInterface2()
		{
			var provider = CreateImageSourceServiceProvider(services =>
			{
				services.AddService<IStreamImageSource, StreamImageSourceService>();
				services.AddService<MultipleInterfacesImageSourceStub, UriImageSourceService>();
			});

			var service = provider.GetRequiredImageSourceService(new DerivedMultipleInterfacesImageSourceStub());
			Assert.IsType<UriImageSourceService>(service);
		}

		class DerivedMultipleInterfacesImageSourceStub : MultipleInterfacesImageSourceStub { }

#pragma warning disable CS0618 // Type or member is obsolete
		[Fact]
		public void GetsImageSourceTypeEvenWhenNoServiceWasRegistered()
		{
			var provider = CreateImageSourceServiceProvider(services => { });

			var imageSource = provider.GetImageSourceType(typeof(FirstImageSource));

			Assert.Equal(typeof(IFirstImageSource), imageSource);
		}

		[Fact]
		public void GetsImageSourceServiceTypeEvenWhenNoServiceWasRegistered()
		{
			var provider = CreateImageSourceServiceProvider(services => { });

			var service = provider.GetImageSourceServiceType(typeof(IFirstImageSource));

			Assert.Equal(typeof(IImageSourceService<IFirstImageSource>), service);
		}

		[Fact]
		public void GetsImageSourceTypeForRegisteredService()
		{
			var provider = CreateImageSourceServiceProvider(services =>
			{
				services.AddService<FirstImageSource, FirstImageSourceService>();
			});

			var service = provider.GetImageSourceType(typeof(FirstImageSource));

			Assert.Equal(typeof(IFirstImageSource), service);
		}

		[Fact]
		public void GetsImageSourceServiceTypeForRegisteredService()
		{
			var provider = CreateImageSourceServiceProvider(services =>
			{
				services.AddService<FirstImageSource, FirstImageSourceService>();
			});

			var service = provider.GetImageSourceServiceType(typeof(FirstImageSource));

			Assert.Equal(typeof(IImageSourceService<FirstImageSource>), service);
		}
#pragma warning restore CS0618 // Type or member is obsolete

		[Fact]
		public void ResolvesCustomDerivedImageSourceOverBuiltInInterface()
		{
			var defaultInstance = new StreamImageSourceService();
			var customInstance = new StreamImageSourceService();

			var provider = CreateImageSourceServiceProvider(services =>
			{
				services.AddService<IStreamImageSource>(_ => defaultInstance);
				services.AddService<StreamImageSourceStub>(_ => customInstance);
			});

			var service = provider.GetImageSourceService(typeof(CustomStreamImageSourceStub));

			Assert.Same(customInstance, service);
		}

		[Fact]
		public void ResolvesImageSourceServiceWhenNonPrimaryInterfaceServiceIsRegistered()
		{
			var parentBImageSourceService = new ChildImageSourceService();

			var provider = CreateImageSourceServiceProvider(svcs =>
			{
				svcs.AddService<IParentBImageSource>(_ => parentBImageSourceService);
			});

			var service = provider.GetRequiredImageSourceService(new ChildImageSource());

			Assert.Same(parentBImageSourceService, service);
		}

		[Fact]
		public void ThrowsWhenMatchingServicesRelatedThroughInheritance()
		{
			var provider = CreateImageSourceServiceProvider(svcs =>
			{
				svcs.AddService<IParentAImageSource, ChildImageSourceService>();
				svcs.AddService<IParentBImageSource, ChildImageSourceService>();
			});

			Assert.Throws<InvalidOperationException>(() => provider.GetRequiredImageSourceService(new ChildImageSource()));
		}

		[Fact]
		public void ImageSourceServiceRegistrationViaAddSingletonDirectlyIsNotSupported()
		{
			var provider = CreateImageSourceServiceProvider(svcs =>
			{
				svcs.AddSingleton<IImageSourceService<IParentAImageSource>, ChildImageSourceService>();
			});

			Assert.Throws<InvalidOperationException>(() => provider.GetRequiredImageSourceService(new ChildImageSource()));
		}

		private class ChildImageSource : IParentAImageSource, IParentBImageSource { public bool IsEmpty => throw new NotImplementedException(); }
		private class ChildImageSourceService : IImageSourceService<IParentAImageSource>, IImageSourceService<IParentBImageSource> { }
		private interface IParentAImageSource : IImageSource { }
		private interface IParentBImageSource : IImageSource { }

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
		private class FirstImageSourceService : IImageSourceService<IFirstImageSource> { }
		private class SecondImageSourceService : IImageSourceService<ISecondImageSource> { }
		private class CombinedImageSourceService : IImageSourceService<IFirstImageSource>, IImageSourceService<ISecondImageSource> { }

		private class CustomStreamImageSourceStub : StreamImageSourceStub { }
	}
}
