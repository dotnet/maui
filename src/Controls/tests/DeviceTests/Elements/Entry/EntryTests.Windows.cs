#nullable enable
using Xunit;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using WTextAlignment = Microsoft.UI.Xaml.TextAlignment;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		static TextBox GetPlatformControl(EntryHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		Task<float> GetPlatformOpacity(EntryHandler entryHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(entryHandler);
				return (float)nativeView.Opacity;
			});
		}

		static void SetPlatformText(EntryHandler entryHandler, string text) =>
			GetPlatformControl(entryHandler).Text = text;

		static int GetPlatformCursorPosition(EntryHandler entryHandler) =>
			GetPlatformControl(entryHandler).SelectionStart;

		static int GetPlatformSelectionLength(EntryHandler entryHandler) =>
			GetPlatformControl(entryHandler).SelectionLength;

		public async Task<WTextAlignment> EntryAlignmentMatchesFlowDirection(bool isExplicit, FlowDirection flowDirection)
		{
			var entry = new Entry { Text = "Checking flow direction", HorizontalTextAlignment = TextAlignment.Start };
			var contentPage = new ContentPage { Title = "Flow Direction", Content = entry };

			if (isExplicit)
			{
				entry.FlowDirection = flowDirection;
			}
			else
			{
				contentPage.FlowDirection = flowDirection;
			}

			var handler = await CreateHandlerAsync<EntryHandler>(entry);
			var nativeAlignment = await contentPage.Dispatcher.DispatchAsync(() =>
			{
				var textField = GetPlatformControl(handler);
				return textField.TextAlignment;
			});
			return nativeAlignment;
		}

		[Fact]
		[Description("The Entry's text alignment should match the expected alignment when FlowDirection is explicitly set to LeftToRight.")]
		public async Task EntryAlignmentMatchesFlowDirectionLtrExplicit()
		{
			var results = await EntryAlignmentMatchesFlowDirection(true, FlowDirection.LeftToRight);
			Assert.Equal(WTextAlignment.Left, results);
		}

		[Fact]
		[Description("The Entry's text alignment should match the expected alignment when FlowDirection is explicitly set to RightToLeft.")]
		public async Task EntryAlignmentMatchesFlowDirectionRtlExplicit()
		{
			var results = await EntryAlignmentMatchesFlowDirection(true, FlowDirection.RightToLeft);
			Assert.Equal(WTextAlignment.Left, results);
		}

		[Fact]
		[Description("The Entry's text alignment should match the expected alignment when FlowDirection is implicitly set to LeftToRight.")]
		public async Task EntryAlignmentMatchesFlowDirectionLtrImplicit()
		{
			var results = await EntryAlignmentMatchesFlowDirection(false, FlowDirection.LeftToRight);
			Assert.Equal(WTextAlignment.Left, results);
		}

		[Fact]
		[Description("The Entry's text alignment should match the expected alignment when FlowDirection is implicitly set to RightToLeft.")]
		public async Task EntryAlignmentMatchesFlowDirectionRtlImplicit()
		{
			var results = await EntryAlignmentMatchesFlowDirection(false, FlowDirection.RightToLeft);
			Assert.Equal(WTextAlignment.Left, results);
		}
	}
}
