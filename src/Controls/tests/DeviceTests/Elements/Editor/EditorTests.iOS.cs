using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
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

		Task<float> GetPlatformOpacity(EditorHandler editorHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(editorHandler);
				return (float)nativeView.Alpha;
			});
		}

		Task<bool> GetPlatformIsVisible(EditorHandler editorHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(editorHandler);
				return !nativeView.Hidden;
			});
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

			[Fact]
			public async Task PlaceholderHorizontalTextAlignment()
			{
				EnsureHandlerCreated(builder =>
				{
					builder.ConfigureMauiHandlers(handlers =>
					{
						handlers.AddHandler(typeof(Editor), typeof(EditorHandler));
					});
				});

				var horizontalEndAlignment = TextAlignment.End;
				var horizontalCenterAlignment = TextAlignment.Center;
				var horizontalStartAlignment = TextAlignment.Start;

				var editor = new Editor
				{
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

					editor.HorizontalTextAlignment = horizontalCenterAlignment;
					Assert.Equal(UIKit.UITextAlignment.Center, placeholderLabel?.TextAlignment);
					editor.HorizontalTextAlignment = horizontalEndAlignment;
					Assert.Equal(UIKit.UITextAlignment.Right, placeholderLabel?.TextAlignment);
					editor.HorizontalTextAlignment = horizontalStartAlignment;
					Assert.Equal(UIKit.UITextAlignment.Left, placeholderLabel?.TextAlignment);

				});
			}
		}

		//src/Compatibility/Core/tests/iOS/FlowDirectionTests.cs
		[Theory]
		[InlineData(true, FlowDirection.LeftToRight, UITextAlignment.Left)]
		[InlineData(true, FlowDirection.RightToLeft, UITextAlignment.Right)]
		[InlineData(false, FlowDirection.LeftToRight, UITextAlignment.Left)]
		[Description("The Editor's text alignment should match the expected alignment when FlowDirection is applied explicitly or implicitly")]
		public async Task EditorAlignmentMatchesFlowDirection(bool isExplicit, FlowDirection flowDirection, UITextAlignment expectedAlignment)
		{
			var editor = new Editor { Text = "Checking flow direction" };
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
			var nativeAlignment = await contentPage.Dispatcher.DispatchAsync(() =>
			{
				var textField = GetPlatformControl(handler);
				return textField.TextAlignment;
			});

			Assert.Equal(expectedAlignment, nativeAlignment);
		}
	}
}
