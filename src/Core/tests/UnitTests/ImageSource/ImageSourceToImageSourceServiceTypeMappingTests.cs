using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Hosting.Internal;
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
		public void ThrowsInCaseOfAmbiguousMatch()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<IFirstImageSource, IImageSourceService<IFirstImageSource>>();
			mapping.Add<ISecondImageSource, IImageSourceService<ISecondImageSource>>();

			Assert.Throws<InvalidOperationException>(() => mapping.FindImageSourceServiceType(typeof(FirstAndSecondImageSource)));
		}

		[Fact]
		public void ReturnExactImageSourceMatch()
		{
			var mapping = new ImageSourceToImageSourceServiceTypeMapping();
			mapping.Add<IFirstImageSource, IImageSourceService<IFirstImageSource>>();
			mapping.Add<ISecondImageSource, IImageSourceService<ISecondImageSource>>();
			mapping.Add<FirstAndSecondImageSource, IImageSourceService<FirstAndSecondImageSource>>();

			var imageSourceServiceType = mapping.FindImageSourceServiceType(typeof(FirstAndSecondImageSource));

			Assert.Equal(typeof(IImageSourceService<FirstAndSecondImageSource>), imageSourceServiceType);
		}

		[Fact]
		public void RegisteredImageSourceServiceCollectionCanBeCollected()
		{
			var collection = RegisterImageSourceService();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert.False(collection.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static WeakReference RegisterImageSourceService()
		{
			var collection = new TestImageSourceServiceCollection();
			collection.AddService<TestImageSource, TestImageSourceService>();

			return new WeakReference(collection);
		}

		private interface IFirstImageSource : IImageSource { }
		private interface ISecondImageSource : IImageSource { }
		private class FirstAndSecondImageSource : IFirstImageSource, ISecondImageSource { public bool IsEmpty => throw new NotImplementedException(); }
		private sealed class TestImageSource : IImageSource { public bool IsEmpty => true; }
		private sealed class TestImageSourceService : IImageSourceService<TestImageSource> { }
		private sealed class TestImageSourceServiceCollection : MauiServiceCollection, IImageSourceServiceCollection { }

		interface IMyCustomImageSource : IFileImageSource { }
		private abstract class MyCustomImageSource : IMyCustomImageSource
		{
			public string File => string.Empty;
			public bool IsEmpty => true;
		}

		private sealed class MyLargeCustomImageSource : MyCustomImageSource { }
		private sealed class MySmallCustomImageSource : MyCustomImageSource { }
	}
}
