#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Xunit;
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

		Task<bool> GetPlatformIsVisible(EditorHandler editorHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(editorHandler);
				return nativeView.Visibility == Microsoft.UI.Xaml.Visibility.Visible;
			});
		}
		//src/Compatibility/Core/tests/WinUI/FlowDirectionTests.cs
		[Theory]
		[InlineData(true, FlowDirection.RightToLeft, WTextAlignment.Left, WFlowDirection.RightToLeft)]
		[InlineData(true, FlowDirection.LeftToRight, WTextAlignment.Left, WFlowDirection.LeftToRight)]
		[InlineData(false, FlowDirection.LeftToRight, WTextAlignment.Left, WFlowDirection.LeftToRight)]
		[Description("The Editor's text alignment and flow direction should match the expected values when FlowDirection is applied explicitly or implicitly.")]
		public async Task EditorAlignmentMatchesFlowDirection(bool isExplicit, FlowDirection flowDirection, WTextAlignment expectedAlignment, WFlowDirection expectedFlowDirection)
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

			Assert.Equal(expectedAlignment, nativeAlignment);
			Assert.Equal(expectedFlowDirection, nativeFlowDirection);
		}
	}
}
