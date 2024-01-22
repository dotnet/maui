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
		public void FindsCorrespondingImageSourceServiceType()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<IStreamImageSource>();
			mapping.Add<IUriImageSource>();

			var type = mapping.FindImageSourceServiceType(typeof(StreamImageSourceStub));
			Assert.Equal(typeof(IImageSourceService<IStreamImageSource>), type);
		}

		[Fact]
		public void PrefersConcreteTypesOverInterfaces()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<ICustomStreamImageSource>();
			mapping.Add<StreamImageSourceStub>();

			var imageSourceType = mapping.FindImageSourceType(typeof(IStreamImageSource));
			var imageSourceServiceType = mapping.FindImageSourceServiceType(typeof(IStreamImageSource));

			Assert.Equal(typeof(StreamImageSourceStub), imageSourceType);
			Assert.Equal(typeof(IImageSourceService<StreamImageSourceStub>), imageSourceServiceType);
		}

		[Fact]
		public void PrefersExactMatches()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<IStreamImageSource>();
			mapping.Add<StreamImageSourceStub>();

			var imageSourceType = mapping.FindImageSourceType(typeof(IStreamImageSource));
			var imageSourceServiceType = mapping.FindImageSourceServiceType(typeof(IStreamImageSource));

			Assert.Equal(typeof(IStreamImageSource), imageSourceType);
			Assert.Equal(typeof(IImageSourceService<IStreamImageSource>), imageSourceServiceType);
		}

		[Fact]
		public void FindsMoreDerivedTypes()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<StreamImageSourceStub>();
			mapping.Add<DerivedStreamImageSourceStub>();

			var imageSourceType = mapping.FindImageSourceType(typeof(IStreamImageSource));
			var imageSourceServiceType = mapping.FindImageSourceServiceType(typeof(IStreamImageSource));

			Assert.Equal(typeof(DerivedStreamImageSourceStub), imageSourceType);
			Assert.Equal(typeof(IImageSourceService<DerivedStreamImageSourceStub>), imageSourceServiceType);
		}

		[Fact]
		public void ThrowsInCaseOfAmbiguity()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<FileImageSourceA>();
			mapping.Add<FileImageSourceB>();

			Assert.Throws<InvalidOperationException>(() => mapping.FindImageSourceType(typeof(IFileImageSource)));
		}

		private interface ICustomStreamImageSource : IStreamImageSource
		{
		}

		private class StreamImageSourceStub : ICustomStreamImageSource
		{
			public Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default) => Task.FromException<Stream>(new NotImplementedException());
			public bool IsEmpty => true;
		}

		private class DerivedStreamImageSourceStub : StreamImageSourceStub
		{
		}

		private class FileImageSourceA : IFileImageSource
		{
			public string File => throw new NotImplementedException();
			public bool IsEmpty => true;
		}

		private class FileImageSourceB : IFileImageSource
		{
			public string File => throw new NotImplementedException();
			public bool IsEmpty => true;
		}
	}
}
