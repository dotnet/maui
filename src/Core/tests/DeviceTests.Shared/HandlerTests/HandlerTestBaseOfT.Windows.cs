using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Media;
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
			Assert.Equal(view.Rotation % 360, r % 360);
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
			Assert.Equal(view.RotationX % 360, rX % 360);
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
			Assert.Equal(view.RotationY % 360, rY % 360);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task InputTransparencyInitializesCorrectly(bool inputTransparent)
		{
			var view = new TStub()
			{
				InputTransparent = inputTransparent
			};

			var uie = await GetValueAsync(view, handler => GetHitTestVisible(handler));

			// HitTestVisible should be the opposite value of InputTransparent 
			if (view is ILayout && inputTransparent)
			{
				// InputTransparent doesn't actually affect hit test visibility for LayoutPanel. 
				// The panel itself needs to always be hit test visible so it can relay input to non-transparent children.
				Assert.True(uie);
				return;
			}

			// HitTestVisible should be the opposite value of InputTransparent 
			Assert.NotEqual(inputTransparent, uie);
		}
	}
}
