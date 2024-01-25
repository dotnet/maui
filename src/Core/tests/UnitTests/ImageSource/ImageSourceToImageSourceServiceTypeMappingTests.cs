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
		public void FindsCorrespondingImageSourceServiceType()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<IStreamImageSource, IImageSourceService<IStreamImageSource>>();
			mapping.Add<IUriImageSource, IImageSourceService<IUriImageSource>>();

			var type = mapping.FindImageSourceServiceType(typeof(StreamImageSourceStub));

			Assert.Equal(typeof(IImageSourceService<IStreamImageSource>), type);
		}

		[Fact]
		public void ThrowsWhenThereIsNoMatchingMapping()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<StreamImageSourceStub, IImageSourceService<StreamImageSourceStub>>();

			Assert.Throws<InvalidOperationException>(() => mapping.FindImageSourceServiceType(typeof(IStreamImageSource)));
		}

		[Theory]
		[InlineData(typeof(IFileImageSource), typeof(IImageSourceService<IFileImageSource>))]
		[InlineData(typeof(MyCustomImageSource), typeof(IImageSourceService<IFileImageSource>))]
		[InlineData(typeof(MyLargeCustomImageSource), typeof(IImageSourceService<MyLargeCustomImageSource>))]
		[InlineData(typeof(MySmallCustomImageSource), typeof(IImageSourceService<MySmallCustomImageSource>))]
		public void FindsClosestApplicableMapping(Type type, Type expectedImageSourceServiceType)
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<IFileImageSource, IImageSourceService<IFileImageSource>>();
			mapping.Add<MyLargeCustomImageSource, IImageSourceService<MyLargeCustomImageSource>>();
			mapping.Add<MySmallCustomImageSource, IImageSourceService<MySmallCustomImageSource>>();

			var imageSourceServiceType = mapping.FindImageSourceServiceType(type);

			Assert.Equal(expectedImageSourceServiceType, imageSourceServiceType);
		}

		[Fact]
		public void FindsMostDerivedBaseInterface()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<IFileImageSource, IImageSourceService<IFileImageSource>>();
			mapping.Add<IMyCustomImageSource, IImageSourceService<IMyCustomImageSource>>();

			var imageSourceServiceType = mapping.FindImageSourceServiceType(typeof(MyCustomImageSource));

			Assert.Equal(typeof(IImageSourceService<IMyCustomImageSource>), imageSourceServiceType);
		}

		[Fact]
        public void DontMatchMoreDerivedTypes()
        {  
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<MyLargeCustomImageSource, IImageSourceService<MyLargeCustomImageSource>>();
			mapping.Add<MySmallCustomImageSource, IImageSourceService<MySmallCustomImageSource>>();

			Assert.Throws<InvalidOperationException>(() => mapping.FindImageSourceServiceType(typeof(MyCustomImageSource)));
        }

		[Fact]
		public void InCaseOfAmbiguousMatchSelectsTheFirstRegisteredService()
		{
			Test<IFileImageSource, IStreamImageSource, MyFileAndStreamImageSource>();
			Test<IStreamImageSource, IFileImageSource, MyFileAndStreamImageSource>();

			void Test<TImageSource1, TImageSource2, TImageSource3>()
				where TImageSource1 : IImageSource
				where TImageSource2 : IImageSource
				where TImageSource3 : class, TImageSource1, TImageSource2
			{
				var mapping = new ImageSourceToImageSourceServiceTypeMapping();
				mapping.Add<TImageSource1, IImageSourceService<TImageSource1>>();
				mapping.Add<TImageSource2, IImageSourceService<TImageSource2>>();

				var imageSourceServiceType = mapping.FindImageSourceServiceType(typeof(TImageSource3));

				Assert.Equal(typeof(IImageSourceService<TImageSource1>), imageSourceServiceType);
			}
		}

		interface IMyCustomImageSource : IFileImageSource { }
		private abstract class MyCustomImageSource : IMyCustomImageSource
		{
			public string File => string.Empty;
			public bool IsEmpty => true;
		}

		private sealed class MyLargeCustomImageSource : MyCustomImageSource { }
		private sealed class MySmallCustomImageSource : MyCustomImageSource { }

		private sealed class MyFileAndStreamImageSource : IFileImageSource, IStreamImageSource
		{
			public string File => throw new NotImplementedException();
			public Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default) => Task.FromException<Stream>(new NotImplementedException());
			public bool IsEmpty => throw new NotImplementedException();
		}
	}
}
