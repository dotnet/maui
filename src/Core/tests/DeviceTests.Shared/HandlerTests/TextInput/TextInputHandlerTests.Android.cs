using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class TextInputHandlerTests<THandler, TStub> : HandlerTestBasement<THandler, TStub>
		where THandler : class, IViewHandler, new()
		where TStub : IStubBase, ITextInputStub, new()
	{
		// This test will only run if the Android Manifest of the Core.DeviceTests project is edited to have android:supportsRtl="false"
		[Fact(DisplayName = "Horizontal text aligned when RTL is not supported")]
		public void HorizontalTextAlignedWhenRtlIsFalse()
		{
			if (Rtl.IsSupported)
				return;

			var inputView = new TStub { Text = "Foo", HorizontalTextAlignment = TextAlignment.Center };

			var handler = CreateHandler<THandler>(inputView);
			TextView platformView;

			if (handler is SearchBarHandler searchBarHandler)
				platformView = searchBarHandler.QueryEditor;
			else
				platformView = (TextView)handler.PlatformView;

			Assert.False(platformView.Gravity.HasFlag(GravityFlags.Start), "Text input should not have the Start flag.");
			Assert.False(platformView.Gravity.HasFlag(GravityFlags.End), "Text input should not have the End flag.");
			Assert.True(platformView.Gravity.HasFlag(GravityFlags.CenterHorizontal), "Text input should have the CenterHorizontal flag.");
		}

		// This test will only run if the Android Manifest of the Core.DeviceTests project is edited to have android:supportsRtl="false"
		[Fact(DisplayName = "Vertical text aligned when RTL is not supported")]
		public void VerticalTextAlignedWhenRtlIsFalse()
		{
			if (Rtl.IsSupported)
				return;

			var inputView = new TStub { Text = "Foo", VerticalTextAlignment = TextAlignment.Center };

			var handler = CreateHandler<THandler>(inputView);
			TextView platformView;

			if (handler is SearchBarHandler searchBarHandler)
				platformView = searchBarHandler.QueryEditor;
			else
				platformView = (TextView)handler.PlatformView;

			Assert.False(platformView.Gravity.HasFlag(GravityFlags.Top), "Search bar editor should not have the Top flag.");
			Assert.False(platformView.Gravity.HasFlag(GravityFlags.Bottom), "Search bar editor should not have the Bottom flag.");
			Assert.True(platformView.Gravity.HasFlag(GravityFlags.CenterVertical), "Search bar editor should only have the CenterVertical flag.");
		}
	}
}
