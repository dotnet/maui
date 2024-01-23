using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.ImageSource
{
	[Category(TestCategory.Core, TestCategory.ImageSource)]
	public class ImageSourceToImageSourceServiceTypeMappingTests
	{
		[Fact]
		public void FindsCorrespondingImageSourceType()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<IStreamImageSource>();
			mapping.Add<IUriImageSource>();

			var type = mapping.FindImageSourceType(typeof(StreamImageSourceStub));
			Assert.Equal(typeof(IStreamImageSource), type);
		}

		[Fact]
		public void FindsCorrespondingImageSourceTypeForDerivedConcreteType()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<IStreamImageSource>();

			var type = mapping.FindImageSourceType(typeof(StreamImageSourceStub));
			Assert.Equal(typeof(IStreamImageSource), type);
		}

		[Fact]
		public void FindsCorrespondingImageSourceServiceType()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<IStreamImageSource>();
			mapping.Add<IUriImageSource>();

			var type = mapping.FindImageSourceServiceType(typeof(StreamImageSourceStub));
			Assert.Equal(typeof(IImageSourceService<IStreamImageSource>), type);
		}

		[Theory]
		[InlineData(typeof(IFileImageSource), typeof(IImageSourceService<IFileImageSource>))]
		[InlineData(typeof(MyCustomImageSource), typeof(IImageSourceService<IFileImageSource>))]
		[InlineData(typeof(MyLargeCustomImageSource), typeof(IImageSourceService<MyLargeCustomImageSource>))]
		[InlineData(typeof(MySmallCustomImageSource), typeof(IImageSourceService<MySmallCustomImageSource>))]
		public void FallsBackToTheImplementedImageSourceInterfaceWhenNotRegistered(Type type, Type expectedImageSourceServiceType)
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<IFileImageSource>();
			mapping.Add<MyLargeCustomImageSource>();
			mapping.Add<MySmallCustomImageSource>();

			var imageSourceType = mapping.FindImageSourceType(type);
			var imageSourceServiceType = mapping.FindImageSourceServiceType(type);

			Assert.Equal(typeof(IFileImageSource), imageSourceType);
			Assert.Equal(expectedImageSourceServiceType, imageSourceServiceType);
		}

		private abstract class MyCustomImageSource : IFileImageSource
		{
			public string File => string.Empty;
			public bool IsEmpty => true;
		}

		private sealed class MyLargeCustomImageSource : MyCustomImageSource { }
		private sealed class MySmallCustomImageSource : MyCustomImageSource { }
	}
}
