using System;
using System.Numerics;
using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase<THandler, TStub>
	{
		[Theory(DisplayName = "TranslationX Initialize Correctly")]
		[InlineData(10)]
		[InlineData(50)]
		[InlineData(100)]
		public async Task TranslationXInitializeCorrectly(double translationX)
		{
			var view = new TStub()
			{
				TranslationX = translationX
			};

			var tX = await GetValueAsync(view, handler => GetTranslationX(handler));
			Assert.Equal(view.TranslationX, tX);
		}

		[Theory(DisplayName = "TranslationY Initialize Correctly")]
		[InlineData(10)]
		[InlineData(50)]
		[InlineData(100)]
		public async Task TranslationYInitializeCorrectly(double translationY)
		{
			var view = new TStub()
			{
				TranslationY = translationY
			};

			var tY = await GetValueAsync(view, handler => GetTranslationY(handler));
			Assert.Equal(view.TranslationY, tY);
		}

		[Theory(DisplayName = "ScaleX Initialize Correctly")]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		public async Task ScaleXInitializeCorrectly(double scaleX)
		{
			var view = new TStub()
			{
				ScaleX = scaleX
			};

			var sX = await GetValueAsync(view, handler => GetScaleX(handler));
			Assert.Equal(view.ScaleX, sX);
		}

		[Theory(DisplayName = "ScaleY Initialize Correctly")]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		public async Task ScaleYInitializeCorrectly(double scaleY)
		{
			var view = new TStub()
			{
				ScaleY = scaleY
			};

			var sY = await GetValueAsync(view, handler => GetScaleY(handler));
			Assert.Equal(view.ScaleY, sY);
		}

		[Theory(DisplayName = "Rotation Initialize Correctly")]
		[InlineData(0)]
		[InlineData(90)]
		[InlineData(180)]
		[InlineData(270)]
		[InlineData(360)]
		public async Task RotationInitializeCorrectly(double rotation)
		{
			var view = new TStub()
			{
				Rotation = rotation
			};

			var r = await GetValueAsync(view, handler => GetRotation(handler));
			Assert.Equal(view.Rotation, r);
		}

		[Theory(DisplayName = "RotationX Initialize Correctly")]
		[InlineData(0)]
		[InlineData(90)]
		[InlineData(180)]
		[InlineData(270)]
		[InlineData(360)]
		public async Task RotationXInitializeCorrectly(double rotationX)
		{
			var view = new TStub()
			{
				RotationX = rotationX
			};

			var rX = await GetValueAsync(view, handler => GetRotationX(handler));
			Assert.Equal(view.RotationX, rX);
		}

		[Theory(DisplayName = "RotationY Initialize Correctly")]
		[InlineData(0)]
		[InlineData(90)]
		[InlineData(180)]
		[InlineData(270)]
		[InlineData(360)]
		public async Task RotationYInitializeCorrectly(double rotationY)
		{
			var view = new TStub()
			{
				RotationY = rotationY
			};

			var rY = await GetValueAsync(view, handler => GetRotationY(handler));
			Assert.Equal(view.RotationY, rY);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(100)]
		public async Task MinimumHeightInitializes(double minHeight)
		{
			var view = new TStub()
			{
				MinimumHeight = minHeight
			};

			var expected = view.MinimumHeight;
			var result = await GetValueAsync(view, handler => GetMinHeight(handler));

			Assert.Equal(expected, result, 0);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(100)]
		public async Task MinimumWidthInitializes(double minWidth)
		{
			var view = new TStub()
			{
				MinimumWidth = minWidth
			};

			var expected = view.MinimumWidth;
			var result = await GetValueAsync(view, handler => GetMinWidth(handler));

			Assert.Equal(expected, result, 0);
		}

		[Fact]
		public async Task NeedsContainerWhenInputTransparent()
		{
			var view = new TStub()
			{
				InputTransparent = true
			};

			var handler = await CreateHandlerAsync(view);

			if (handler is LayoutHandler lh)
				Assert.False(lh.NeedsContainer);
			else if (handler is ViewHandler vh)
				Assert.True(vh.NeedsContainer);
		}
	}
}