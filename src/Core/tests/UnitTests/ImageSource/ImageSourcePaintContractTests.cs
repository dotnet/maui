using System;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.UnitTests.ImageSource
{
	/// <summary>
	/// Verifies the public <see cref="IImageSourcePaint"/> contract that out-of-tree platform backends
	/// consume to detect and read image-source backgrounds. The contract is consumption-only, so these
	/// tests never implement it outside of .NET MAUI.
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

		[Fact]
		public void ExternalBackendReadsNullImageSourceAsAnImagePaint()
		{
			// A null ImageSource still identifies an image background; it simply has nothing to draw.
			var view = new ViewStub { Background = new ImageSourcePaint() };

			var result = FakeExternalBackend.Describe(view);

			Assert.Equal(FakeExternalBackend.PaintKind.Image, result.Kind);
			Assert.Null(result.ImageSource);
		}

		[Fact]
		public void ImagePaintIsNotAnImageSourcePaint()
		{
			// Graphics.ImagePaint carries an already-loaded IImage and is a distinct concept.
			var view = new ViewStub { Background = new ImagePaint() };

			var result = FakeExternalBackend.Describe(view);

			Assert.NotEqual(FakeExternalBackend.PaintKind.Image, result.Kind);
			Assert.IsNotAssignableFrom<IImageSourcePaint>(view.Background);
		}

		/// <summary>
		/// Stands in for an out-of-tree platform backend. It only consumes the contract - it never implements
		/// it - and every member it touches is public .NET MAUI API, so the image source is retrieved with no
		/// reflection and no internals access.
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
