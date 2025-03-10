#nullable enable
using Xunit;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using WFlowDirection = Microsoft.UI.Xaml.FlowDirection;
using WTextAlignment = Microsoft.UI.Xaml.TextAlignment;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EditorTests
	{
		static TextBox GetPlatformControl(EditorHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(EditorHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		Task<float> GetPlatformOpacity(EditorHandler editorHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(editorHandler);
				return (float)nativeView.Opacity;
			});
		}

		static void SetPlatformText(EditorHandler editorHandler, string text) =>
			GetPlatformControl(editorHandler).Text = text;

		static int GetPlatformCursorPosition(EditorHandler editorHandler) =>
			GetPlatformControl(editorHandler).SelectionStart;

		static int GetPlatformSelectionLength(EditorHandler editorHandler) =>
			GetPlatformControl(editorHandler).SelectionLength;

		private async Task<Tuple<WTextAlignment, WFlowDirection>> GetEditorAlignmentAndFlowDirection(bool isExplicit, FlowDirection flowDirection)
		{
			var editor = new Editor { Text = " تسجيل الدخول" };
			var contentPage = new ContentPage { Title = "Flow Direction", Content = editor };

			if (isExplicit)
			{
				editor.FlowDirection = flowDirection;
			}
			else
			{
				contentPage.FlowDirection = flowDirection;
			}
			var handler = await CreateHandlerAsync<EditorHandler>(editor);
			var (nativeAlignment, nativeFlowDirection) = await contentPage.Dispatcher.DispatchAsync(() =>
			{
				var textField = GetPlatformControl(handler);
				return (textField.TextAlignment, textField.FlowDirection);
			});
			return new Tuple<WTextAlignment, WFlowDirection>(nativeAlignment, nativeFlowDirection);
		}

		[Fact]
		[Description("The Editor's text alignment and flow direction should match the expected values when FlowDirection is explicitly set to RightToLeft.")]
		public async Task EditorAlignmentMatchesFlowDirectionRtlExplicit()
		{
			var results = await GetEditorAlignmentAndFlowDirection(true, FlowDirection.RightToLeft);
			Assert.Equal(WTextAlignment.Left, results.Item1);
			Assert.Equal(WFlowDirection.RightToLeft, results.Item2);
		}

		[Fact]
		[Description("The Editor's text alignment and flow direction should match the expected values when FlowDirection is explicitly set to LeftToRight.")]
		public async Task EditorAlignmentMatchesFlowDirectionLtrExplicit()
		{
			var results = await GetEditorAlignmentAndFlowDirection(true, FlowDirection.LeftToRight);
			Assert.Equal(WTextAlignment.Left, results.Item1);
			Assert.Equal(WFlowDirection.LeftToRight, results.Item2);
		}

		[Fact]
		[Description("The Editor's text alignment and flow direction should match the expected values when FlowDirection is implicitly set to LeftToRight.")]
		public async Task EditorAlignmentMatchesFlowDirectionLtrImplicit()
		{
			var results = await GetEditorAlignmentAndFlowDirection(false, FlowDirection.LeftToRight);
			Assert.Equal(WTextAlignment.Left, results.Item1);
			Assert.Equal(WFlowDirection.LeftToRight, results.Item2);
		}
	}
}
