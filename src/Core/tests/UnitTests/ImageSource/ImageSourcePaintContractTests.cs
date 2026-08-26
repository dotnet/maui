using System;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.UnitTests.ImageSource
{
	/// <summary>
	/// Verifies the public <see cref="IImageSourcePaint"/> contract that out-of-tree platform backends
	/// rely on to detect and render image-source backgrounds.
	/// </summary>
	[Category(TestCategory.Core)]
	public class ImageSourcePaintContractTests
	{
		[Fact]
		public void ContractIsVisibleToExternalAssemblies()
		{
			var contract = typeof(IImageSourcePaint);

			Assert.True(contract.IsInterface);
			Assert.True(contract.IsPublic, "IImageSourcePaint must be public so external backends can reference it.");
			Assert.Equal("Microsoft.Maui.IImageSourcePaint", contract.FullName);
		}

		[Fact]
		public void ContractExposesOnlyTheImageSource()
		{
			var members = typeof(IImageSourcePaint).GetMembers(BindingFlags.Public | BindingFlags.Instance);

			var property = Assert.Single(typeof(IImageSourcePaint).GetProperties());
			Assert.Equal(nameof(IImageSourcePaint.ImageSource), property.Name);
			Assert.Equal(typeof(IImageSource), property.PropertyType);
			Assert.NotNull(property.GetMethod);
			Assert.Null(property.SetMethod);

			// Only the getter should be projected onto the contract; no other implementation detail leaks out.
			Assert.Equal(new[] { property.GetMethod }, members.OfType<MethodInfo>());
		}

		[Fact]
		public void BuiltInImageSourcePaintImplementsTheContractAndStaysInternal()
		{
			var paintType = typeof(IView).Assembly.GetType("Microsoft.Maui.ImageSourcePaint", throwOnError: true);

			Assert.False(paintType.IsVisible, "ImageSourcePaint should remain an implementation detail.");
			Assert.True(typeof(IImageSourcePaint).IsAssignableFrom(paintType));
			Assert.True(typeof(Paint).IsAssignableFrom(paintType));
		}

		[Fact]
		public void ExternalBackendCanReadImageSourceFromBuiltInPaint()
		{
			var imageSource = new ImageSourceStub();
			var view = new ViewStub { Background = new ImageSourcePaint(imageSource) };

			var result = FakeExternalBackend.Describe(view);

			Assert.Equal(FakeExternalBackend.PaintKind.Image, result.Kind);
			Assert.Same(imageSource, result.ImageSource);
		}

		[Fact]
		public void ExternalBackendCanReadImageSourceFromExternalPaint()
		{
			var imageSource = new ExternalImageSource();
			var view = new ViewStub { Background = new ExternalImageSourcePaint(imageSource) };

			var result = FakeExternalBackend.Describe(view);

			Assert.Equal(FakeExternalBackend.PaintKind.Image, result.Kind);
			Assert.Same(imageSource, result.ImageSource);
		}

		[Fact]
		public void ExternalBackendDistinguishesSolidPaint()
		{
			var view = new ViewStub { Background = new SolidPaint(Colors.Red) };

			var result = FakeExternalBackend.Describe(view);

			Assert.Equal(FakeExternalBackend.PaintKind.Solid, result.Kind);
			Assert.Null(result.ImageSource);
		}

		[Theory]
		[InlineData(typeof(LinearGradientPaint))]
		[InlineData(typeof(RadialGradientPaint))]
		public void ExternalBackendDistinguishesGradientPaint(Type gradientPaintType)
		{
			var view = new ViewStub { Background = (Paint)Activator.CreateInstance(gradientPaintType) };

			var result = FakeExternalBackend.Describe(view);

			Assert.Equal(FakeExternalBackend.PaintKind.Gradient, result.Kind);
			Assert.Null(result.ImageSource);
		}

		[Fact]
		public void ExternalBackendDistinguishesNoPaint()
		{
			var result = FakeExternalBackend.Describe(new ViewStub());

			Assert.Equal(FakeExternalBackend.PaintKind.None, result.Kind);
			Assert.Null(result.ImageSource);
		}

		/// <summary>
		/// A paint authored entirely outside of .NET MAUI. It only depends on public API, which proves a
		/// third-party backend can both produce and consume image-source backgrounds.
		/// </summary>
		class ExternalImageSourcePaint : Paint, IImageSourcePaint
		{
			public ExternalImageSourcePaint(IImageSource imageSource) => ImageSource = imageSource;

			public IImageSource ImageSource { get; }
		}

		class ExternalImageSource : IImageSource
		{
			public bool IsEmpty => false;
		}

		/// <summary>
		/// Stands in for an out-of-tree platform backend. Every member it touches is public .NET MAUI API and
		/// the image source is retrieved through the contract - no reflection and no internals access.
		/// </summary>
		static class FakeExternalBackend
		{
			public enum PaintKind
			{
				None,
				Solid,
				Gradient,
				Image,
			}

			public static (PaintKind Kind, IImageSource ImageSource) Describe(IView view)
			{
				switch (view.Background)
				{
					case IImageSourcePaint imagePaint:
						return (PaintKind.Image, imagePaint.ImageSource);
					case GradientPaint:
						return (PaintKind.Gradient, null);
					case SolidPaint:
						return (PaintKind.Solid, null);
					default:
						return (PaintKind.None, null);
				}
			}
		}
	}
}
