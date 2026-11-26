#nullable enable

using System.Reflection;
using Android.Graphics;
using Android.Graphics.Drawables;
using Microsoft.Maui.Maps.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Map)]
	public class MapIconTests
	{
		[Theory]
		[InlineData(200, 200, 100, 100)]
		[InlineData(200, 100, 100, 50)]
		[InlineData(40, 80, 50, 100)]
		[InlineData(100, 100, 100, 100)]
		public void BitmapDrawableIsScaledToMarkerSize(int width, int height, int expectedWidth, int expectedHeight)
		{
			using var source = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888!);
			using var drawable = new BitmapDrawable(global::Android.App.Application.Context.Resources, source);
			using var result = ConvertDrawable(drawable);

			Assert.Equal(expectedWidth, result.Width);
			Assert.Equal(expectedHeight, result.Height);
			Assert.NotSame(source, result);
		}

		[Fact]
		public void DrawableWithIntrinsicSizePreservesAspectRatio()
		{
			using var drawable = new GradientDrawable();
			drawable.SetSize(200, 100);
			using var result = ConvertDrawable(drawable);

			Assert.Equal(100, result.Width);
			Assert.Equal(50, result.Height);
		}

		[Fact]
		public void DrawableWithoutIntrinsicSizeUsesMarkerSize()
		{
			using var drawable = new ColorDrawable(Color.Red);
			using var result = ConvertDrawable(drawable);

			Assert.Equal(100, result.Width);
			Assert.Equal(100, result.Height);
		}

		static Bitmap ConvertDrawable(Drawable drawable)
		{
			var method = typeof(MapHandler).GetMethod("DrawableToBitmap", BindingFlags.Static | BindingFlags.NonPublic);
			Assert.NotNull(method);
			return Assert.IsType<Bitmap>(method.Invoke(null, new object[] { drawable }));
		}
	}
}
