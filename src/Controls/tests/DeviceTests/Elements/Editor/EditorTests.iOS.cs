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

		[Theory]
		[InlineData(TextAlignment.Start, TextAlignment.Start)]
		[InlineData(TextAlignment.Center, TextAlignment.Start)]
		[InlineData(TextAlignment.End, TextAlignment.Start)]
		[InlineData(TextAlignment.Start, TextAlignment.Center)]
		[InlineData(TextAlignment.Center, TextAlignment.Center)]
		[InlineData(TextAlignment.End, TextAlignment.Center)]
		[InlineData(TextAlignment.Start, TextAlignment.End)]
		[InlineData(TextAlignment.Center, TextAlignment.End)]
		[InlineData(TextAlignment.End, TextAlignment.End)]
		[Description("Editor height should honour HeightRequest for all text alignment combinations when placed in an infinite height constraint layout.")]
		public async Task EditorHeightIsConsistentAcrossAllTextAlignments(TextAlignment horizontal, TextAlignment vertical)
		{
			SetupBuilder();

			const double heightRequest = 100;

			var editor = new Editor
			{
				Text = "testing",
				HeightRequest = heightRequest,
				HorizontalTextAlignment = horizontal,
				VerticalTextAlignment = vertical,
			};

			var layout = new VerticalStackLayout
			{
				Children = { editor }
			};

			await AttachAndRun<LayoutHandler>(layout, async (_) =>
			{
				var frame = editor.Frame;
				await WaitForUIUpdate(frame, editor);

				Assert.Equal(heightRequest, editor.Height, tolerance: 1.0);
			});
		}

		[Fact]
		[Description("Editor with AutoSize=TextChanges should continue to grow after a simulated rotation (width constraint change)")]
		public async Task AutoSizeTextChangesEditorGrowsAfterRotation()
		{
			// Regression test for https://github.com/dotnet/maui/issues/35114
			// Verifies that AllowAutoGrowth remains true after a width constraint change
			// (which is what happens internally during portrait↔landscape rotation).
			SetupBuilder();

			var editor = new Editor
			{
				AutoSize = EditorAutoSizeOption.TextChanges,
				Text = "Line1\nLine2\nLine3",
				WidthRequest = 200,
			};

			var layout = new VerticalStackLayout
			{
				WidthRequest = 200,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
				Children = { editor }
			};

			await AttachAndRun<LayoutHandler>(layout, async (_) =>
			{
				var frame = editor.Frame;
				await WaitForUIUpdate(frame, editor);
				var heightBeforeRotation = editor.Height;

				// Simulate rotation: portrait → landscape (widen) → portrait (narrow)
				frame = editor.Frame;
				layout.WidthRequest = 400;
				await WaitForUIUpdate(frame, editor);

				frame = editor.Frame;
				layout.WidthRequest = 200;
				await WaitForUIUpdate(frame, editor);

				// After simulated rotation, typing more text should still grow the editor
				frame = editor.Frame;
				editor.Text += "\nLine4\nLine5\nLine6";
				await WaitForUIUpdate(frame, editor);

				Assert.True(editor.Height > heightBeforeRotation,
					$"Editor with AutoSize=TextChanges should still grow after rotation. Before: {heightBeforeRotation}, After: {editor.Height}");
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
