using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public partial class EditorTests
	{
		static MauiTextView GetPlatformControl(EditorHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(EditorHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		static void SetPlatformText(EditorHandler editorHandler, string text) =>
			GetPlatformControl(editorHandler).Text = text;

		static int GetPlatformCursorPosition(EditorHandler editorHandler)
		{
			var nativeEditor = GetPlatformControl(editorHandler);

			if (nativeEditor != null && nativeEditor.SelectedTextRange != null)
				return (int)nativeEditor.GetOffsetFromPosition(nativeEditor.BeginningOfDocument, nativeEditor.SelectedTextRange.Start);

			return -1;
		}

		static int GetPlatformSelectionLength(EditorHandler editorHandler)
		{
			var nativeEditor = GetPlatformControl(editorHandler);

			if (nativeEditor != null && nativeEditor.SelectedTextRange != null)
				return (int)nativeEditor.GetOffsetFromPosition(nativeEditor.SelectedTextRange.Start, nativeEditor.SelectedTextRange.End);

			return -1;
		}

		[Category(TestCategory.Editor)]
		public class PlaceholderTests : ControlsHandlerTestBase
		{
			[Fact]
			public async Task PlaceholderFontFamily()
			{
				EnsureHandlerCreated(builder =>
				{
					builder.ConfigureMauiHandlers(handlers =>
					{
						handlers.AddHandler(typeof(Editor), typeof(EditorHandler));
					});
				});

				var expectedFontFamily = "Times New Roman";

				var editor = new Editor
				{
					FontFamily = expectedFontFamily,
					Placeholder = "This is a placeholder"
				};

				ContentPage contentPage = new ContentPage()
				{
					Content = new VerticalStackLayout()
					{
						editor
					}
				};

				await CreateHandlerAndAddToWindow(contentPage, async () =>
				{
					await AssertEventually(() => editor.IsVisible);
					var handler = CreateHandler<EditorHandler>(editor);
					var platformControl = GetPlatformControl(handler);

					var placeholderLabel = handler.PlatformView.Subviews.OfType<UIKit.UILabel>().FirstOrDefault();

					Assert.Equal(expectedFontFamily, placeholderLabel?.Font?.FamilyName);
				});
			}
		}
	}
}
