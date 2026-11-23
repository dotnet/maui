using System;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using ObjCRuntime;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryHandlerTests
	{
		[Fact(DisplayName = "Horizontal TextAlignment Initializes Correctly")]
		public async Task HorizontalTextAlignmentInitializesCorrectly()
		{
			var xplatHorizontalTextAlignment = TextAlignment.End;

			var entry = new EntryStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment
			};

			UITextAlignment expectedValue = UITextAlignment.Right;

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.HorizontalTextAlignment,
					PlatformViewValue = GetNativeHorizontalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);
			values.PlatformViewValue.AssertHasFlag(expectedValue);
		}

		[Fact(DisplayName = "ReturnType Initializes Correctly")]
		public async Task ReturnTypeInitializesCorrectly()
		{
			var xplatReturnType = ReturnType.Next;
			var entry = new EntryStub()
			{
				Text = "Test",
				ReturnType = xplatReturnType
			};

			UIReturnKeyType expectedValue = UIReturnKeyType.Next;

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.ReturnType,
					PlatformViewValue = GetNativeReturnType(handler)
				};
			});

			Assert.Equal(xplatReturnType, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			string originalText = "Some Test Text";
			var xplatCharacterSpacing = 4;

			var entry = new EntryStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = originalText
			};

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.PlatformViewValue);
		}

		[Fact(DisplayName = "Clear button image resets when TextColor is null")]
		public async Task ClearButtonImageResetsWhenTextColorIsNull()
		{
			EntryStub entry = new EntryStub
			{
				Text = "MAUI",
				ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
				TextColor = null
			};

			await AttachAndRun(entry, async (handler) =>
			{
				await AssertEventually(() => handler.PlatformView.IsLoaded());
				Assert.True(handler.PlatformView.BecomeFirstResponder());
				await AssertEventually(() => handler.PlatformView.IsFirstResponder);

				var clearButton = GetNativeClearButton(handler);
				Assert.NotNull(clearButton);

				var defaultImage = clearButton.ImageForState(UIControlState.Normal);
				Assert.NotNull(defaultImage);
				Assert.Equal(UIImageRenderingMode.AlwaysOriginal, defaultImage.RenderingMode);

				entry.TextColor = Colors.Purple;
				handler.UpdateValue(nameof(IEntry.TextColor));

				var tintedImage = clearButton.ImageForState(UIControlState.Normal);
				Assert.NotNull(tintedImage);
				Assert.Equal(UIImageRenderingMode.Automatic, tintedImage.RenderingMode);

				entry.TextColor = null;
				handler.UpdateValue(nameof(IEntry.TextColor));

				// UIKit restores the original AlwaysOriginal system image when SetImage(null) is called
				// on this private clearButton — ImageForState(.Highlighted) must return non-null for re-tinting to work.
				var resetImage = clearButton.ImageForState(UIControlState.Normal);
				Assert.NotNull(resetImage);
				Assert.Equal(UIImageRenderingMode.AlwaysOriginal, resetImage.RenderingMode);

				entry.TextColor = Colors.Blue;
				handler.UpdateValue(nameof(IEntry.TextColor));

				// Verify re-tinting works after reset (null→color→null→color)
				// Confirms ImageForState(.Highlighted) returns the original after SetImage(null)
				var retintedImage = clearButton.ImageForState(UIControlState.Normal);
				Assert.NotNull(retintedImage);
				Assert.Equal(UIImageRenderingMode.Automatic, retintedImage.RenderingMode);
			});
		}

		[Fact(DisplayName = "Clear button tint matches TextColor at full opacity")]
		public async Task ClearButtonTintMatchesTextColorAtFullOpacity()
		{
			EntryStub entry = new EntryStub
			{
				Text = "MAUI",
				ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
				TextColor = null
			};

			await AttachAndRun(entry, async (handler) =>
			{
				await AssertEventually(() => handler.PlatformView.IsLoaded());
				Assert.True(handler.PlatformView.BecomeFirstResponder());
				await AssertEventually(() => handler.PlatformView.IsFirstResponder);

				var clearButton = GetNativeClearButton(handler);
				Assert.NotNull(clearButton);

				entry.TextColor = Colors.Blue;
				handler.UpdateValue(nameof(IEntry.TextColor));

				var tintedImage = clearButton.ImageForState(UIControlState.Normal);
				Assert.NotNull(tintedImage);

				Assert.Equal(UIImageRenderingMode.Automatic, tintedImage.RenderingMode);

				// the clear button was tinted from the system symbol image, which UIImage.Draw re-rasterizes
				// at reduced opacity (~20%), so the glyph never reached full opacity and looked dimmed.
				// The tinted glyph must contain fully opaque pixels so it matches TextColor.
				Assert.Equal(byte.MaxValue, GetMaxAlpha(tintedImage));
			});
		}

		[Fact]
		public async Task NextMovesToNextEntry()
		{
			var entry1 = new EntryStub
			{
				Text = "Entry 1",
				ReturnType = ReturnType.Next
			};

			var entry2 = new EntryStub
			{
				Text = "Entry 2",
				ReturnType = ReturnType.Next
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
				Assert.True(entry2.IsFocused);
			}, entry1, entry2);
		}

		[Fact]
		public async Task NextMovesPastNotEnabledEntry()
		{
			var entry1 = new EntryStub
			{
				Text = "Entry 1",
				ReturnType = ReturnType.Next
			};

			var entry2 = new EntryStub
			{
				Text = "Entry 2",
				ReturnType = ReturnType.Next,
				IsEnabled = false
			};

			var entry3 = new EntryStub
			{
				Text = "Entry 2",
				ReturnType = ReturnType.Next
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
				Assert.True(entry3.IsFocused);
			}, entry1, entry2, entry3);
		}

		[Fact]
		public async Task NextMovesToEditor()
		{
			var entry = new EntryStub
			{
				Text = "Entry",
				ReturnType = ReturnType.Next
			};

			var editor = new EditorStub
			{
				Text = "Editor"
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry.ToPlatform(), customSuperView: entry.ToPlatform().Superview);
				Assert.True(editor.IsFocused);
			}, entry, editor);
		}

		[Fact]
		public async Task NextMovesPastNotEnabledEditor()
		{
			var entry = new EntryStub
			{
				Text = "Entry",
				ReturnType = ReturnType.Next
			};

			var editor1 = new EditorStub
			{
				Text = "Editor1",
				IsEnabled = false
			};

			var editor2 = new EditorStub
			{
				Text = "Editor2"
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry.ToPlatform(), customSuperView: entry.ToPlatform().Superview);
				Assert.True(editor2.IsFocused);
			}, entry, editor1, editor2);
		}

		[Fact]
		public async Task NextMovesToSearchBar()
		{
			var entry = new EntryStub
			{
				Text = "Entry",
				ReturnType = ReturnType.Next
			};

			var searchBar = new SearchBarStub
			{
				Text = "Search Bar"
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry.ToPlatform(), customSuperView: entry.ToPlatform().Superview);
				var uISearchBar = searchBar.Handler.PlatformView as UISearchBar;
				Assert.True(uISearchBar.GetSearchTextField().IsFirstResponder);
			}, entry, searchBar);
		}

		[Fact]
		public async Task NextMovesRightToLeftEntry()
		{
			var hsl = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.RightToLeft
			};

			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			hsl.Add(entry1);
			hsl.Add(entry2);

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: hsl.ToPlatform().Superview);
				var entry1Rect = entry1.ToPlatform().ConvertRectToView(entry1.ToPlatform().Bounds, hsl.ToPlatform());
				var entry2Rect = entry2.ToPlatform().ConvertRectToView(entry2.ToPlatform().Bounds, hsl.ToPlatform());
				Assert.True(entry1Rect.Right > entry2Rect.Right);
				Assert.True(entry2.IsFocused);
			}, hsl);
		}

		[Fact]
		public async Task NextMovesRightToLeftMultilineEntry()
		{
			var hsl1 = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.RightToLeft
			};

			var hsl2 = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.RightToLeft
			};

			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			var entry3 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			var entry4 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			hsl1.Add(entry1);
			hsl1.Add(entry2);
			hsl2.Add(entry3);
			hsl2.Add(entry4);

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry2.ToPlatform(), customSuperView: hsl1.ToPlatform().Superview);
				var entry2Rect = entry2.ToPlatform().ConvertRectToView(entry2.ToPlatform().Bounds, hsl1.ToPlatform());
				var entry3Rect = entry3.ToPlatform().ConvertRectToView(entry3.ToPlatform().Bounds, hsl2.ToPlatform());
				Assert.True(entry2Rect.Right < entry3Rect.Right);
				Assert.True(entry3.IsFocused);
			}, hsl1, hsl2);
		}

		[Fact]
		public async Task NextMovesLtrToRtlMultilineEntry()
		{
			var hsl1 = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.LeftToRight
			};

			var hsl2 = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.RightToLeft
			};

			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			var entry3 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			var entry4 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				Width = 25
			};

			hsl1.Add(entry1);
			hsl1.Add(entry2);
			hsl2.Add(entry3);
			hsl2.Add(entry4);

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry2.ToPlatform(), customSuperView: hsl1.ToPlatform().Superview);
				var entry2Rect = entry2.ToPlatform().ConvertRectToView(entry2.ToPlatform().Bounds, hsl1.ToPlatform());
				var entry3Rect = entry3.ToPlatform().ConvertRectToView(entry3.ToPlatform().Bounds, hsl2.ToPlatform());
				Assert.True(entry2Rect.Right < entry3Rect.Right);
				Assert.True(entry3.IsFocused);
			}, hsl1, hsl2);
		}

		[Fact]
		public async Task NextMovesRtlToLtrMultilineEntry()
		{
			var hsl1 = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.RightToLeft
			};

			var hsl2 = new HorizontalStackLayoutStub
			{
				FlowDirection = FlowDirection.LeftToRight
			};

			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var entry3 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var entry4 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			hsl1.Add(entry1);
			hsl1.Add(entry2);
			hsl2.Add(entry3);
			hsl2.Add(entry4);

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry2.ToPlatform(), customSuperView: hsl1.ToPlatform().Superview);
				var entry2Rect = entry2.ToPlatform().ConvertRectToView(entry2.ToPlatform().Bounds, hsl1.ToPlatform());
				var entry3Rect = entry3.ToPlatform().ConvertRectToView(entry3.ToPlatform().Bounds, hsl2.ToPlatform());
				Assert.True(entry2Rect.Right > entry3Rect.Right);
				Assert.True(entry3.IsFocused);
			}, hsl1, hsl2);
		}

		[Fact]
		public async Task NextMovesBackToTop()
		{
			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry2.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
				Assert.True(entry1.IsFocused);
			}, entry1, entry2);
		}

		[Fact]
		public async Task NextMovesBackToTopIgnoringNotEnabled()
		{
			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next,
				IsEnabled = false
			};

			var editor = new EntryStub
			{
				ReturnType = ReturnType.Next,
				IsEnabled = false
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var entry3 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry3.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
				Assert.True(entry2.IsFocused);
			}, entry1, editor, entry2, entry3);
		}

		[Fact]
		public async Task NextMovesSkipsHiddenSibling()
		{
			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next,
			};

			var entry2 = new EntryStub
			{
				Visibility = Visibility.Hidden,
				ReturnType = ReturnType.Next,
			};

			var entry3 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: entry1.ToPlatform().Superview);
				Assert.True(entry3.IsFocused);
			}, entry1, entry2, entry3);
		}

		[Fact]
		public async Task NextMovesSkipsHiddenParent()
		{
			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next,
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next,
			};

			var entry3 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var vsl1 = new VerticalStackLayoutStub();
			vsl1.Add(entry1);
			var vsl2 = new VerticalStackLayoutStub { Visibility = Visibility.Hidden };
			vsl2.Add(entry2);
			var vsl3 = new VerticalStackLayoutStub();
			vsl3.Add(entry3);

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: vsl1.ToPlatform().Superview);
				Assert.True(entry3.IsFocused);
			}, vsl1, vsl2, vsl3);
		}

		[Fact]
		public async Task NextMovesSkipsHiddenAncestor()
		{
			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next,
			};

			var entry2 = new EntryStub
			{
				ReturnType = ReturnType.Next,
			};

			var entry3 = new EntryStub
			{
				ReturnType = ReturnType.Next
			};

			var vsl1 = new VerticalStackLayoutStub();
			vsl1.Add(entry1);
			var vsl2 = new VerticalStackLayoutStub { Visibility = Visibility.Hidden };
			var vsl2_1 = new VerticalStackLayoutStub();
			var vsl2_2 = new VerticalStackLayoutStub();
			vsl2.Add(vsl2_1);
			vsl2_1.Add(vsl2_2);
			vsl2_2.Add(entry2);
			var vsl3 = new VerticalStackLayoutStub();
			vsl3.Add(entry3);

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: vsl1.ToPlatform().Superview);
				Assert.True(entry3.IsFocused);
			}, vsl1, vsl2, vsl3);
		}

		[Fact]
		public async Task NextMovesSkipsReadonlyFields()
		{
			var entry1 = new EntryStub
			{
				ReturnType = ReturnType.Next,
			};

			var editor1 = new EditorStub
			{
				IsReadOnly = true
			};

			var editor2 = new EditorStub();

			var vsl = new VerticalStackLayoutStub
			{
				entry1,
				editor1,
				editor2
			};

			await NextMovesHelper(() =>
			{
				KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform(), customSuperView: vsl.ToPlatform().Superview);
				Assert.True(editor2.IsFocused);
			}, vsl);
		}

		async Task NextMovesHelper(Action action = null, params StubBase[] views)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler<VerticalStackLayoutStub, LayoutHandler>();
					handler.AddHandler<HorizontalStackLayoutStub, LayoutHandler>();
					handler.AddHandler<EntryStub, EntryHandler>();
					handler.AddHandler<EditorStub, EditorHandler>();
					handler.AddHandler<SearchBarStub, SearchBarHandler>();
				});
			});

			var layout = new VerticalStackLayoutStub();

			foreach (var view in views)
			{
				layout.Add(view);
			}

			layout.Width = 300;
			layout.Height = 150;

			await InvokeOnMainThreadAsync(async () =>
			{
				var contentViewHandler = CreateHandler<LayoutHandler>(layout);
				await contentViewHandler.PlatformView.AttachAndRun(() =>
				{
					action?.Invoke();
				});
			});
		}

		[Fact]
		public async Task ScrollEntry()
		{
			var entry1 = new EntryStub
			{
				Height = 600
			};

			var entry2 = new EntryStub
			{
				Height = 200
			};

			await ScrollHelper(async () => await ScrollToText(entry2), entry1, entry2);
		}

		[Fact]
		public async Task ScrollEditor()
		{
			var entry = new EntryStub
			{
				Height = 600
			};

			var editor = new EditorStub
			{
				Height = 200
			};

			await ScrollHelper(async () => await ScrollToText(editor), entry, editor);
		}

		[Fact]
		public async Task ScrollNextEntry()
		{
			var entry1 = new EntryStub
			{
				Height = 600,
				ReturnType = ReturnType.Next
			};

			var entry2 = new EntryStub
			{
				Height = 200
			};

			await ScrollHelper(async () => await ScrollToNext(entry1, entry2), entry1, entry2);
		}

		[Fact]
		public async Task ScrollNextEditor()
		{
			var entry = new EntryStub
			{
				Height = 600,
				ReturnType = ReturnType.Next
			};

			var editor = new EditorStub
			{
				Height = 200
			};

			await ScrollHelper(async () => await ScrollToNext(entry, editor), entry, editor);
		}

		[Fact]
		public async Task CursorPositionPreservedDuringInsertTextWithTextTransformUppercase()
		{
			var entry = new EntryStub();

			await AttachAndRun(entry, (handler) =>
			{
				var textField = GetNativeEntry(handler);

				textField.BecomeFirstResponder();

				foreach (var c in "hello")
				{
					textField.InsertText(c.ToString());
				}

				UpdateCursorStartPosition(handler, 2);
				Assert.Equal(2, GetCursorStartPosition(handler));

				textField.InsertText("x");
				Assert.Equal(3, GetCursorStartPosition(handler));
			});
		}

		[Fact(DisplayName = "Turning IsPassword On While Focused Masks Every Character")]
		public async Task TurningIsPasswordOnWhileFocusedMasksEveryCharacter()
		{
			var entry = new EntryStub
			{
				Text = "DemoTest1",
				IsPassword = false,
				Font = Font.SystemFontOfSize(32),
				TextColor = Colors.Red,
				Width = 400, // wide enough to show every character
			};

			await AttachAndRun(entry, async (handler) =>
			{
				var textField = GetNativeEntry(handler);
				await AssertEventually(() => textField.IsLoaded());
				Assert.True(textField.BecomeFirstResponder());
				await AssertEventually(() => textField.IsFirstResponder);

				entry.IsPassword = true;
				handler.UpdateValue(nameof(IEntry.IsPassword));

				// UIKit shows a typed character for about 2 seconds before masking it, so a reveal is still on screen here
				await Wait(() => GetRenderedGlyphs(textField).StartsWith("••••••••", StringComparison.Ordinal));
				Assert.Equal("•••••••••", GetRenderedGlyphs(textField));

				// Typing next must still add to the text rather than replace it (https://github.com/dotnet/maui/issues/30085)
				textField.InsertText("x");
				Assert.Equal("DemoTest1x", textField.Text);
			});
		}

		// One character per glyph drawn in the text color: '•' for a filled dot, '?' for anything else.
		static string GetRenderedGlyphs(UITextField textField)
		{
			var size = textField.Bounds.Size;
			using var image = new UIGraphicsImageRenderer(size, new UIGraphicsImageRendererFormat { Scale = 3 })
				.CreateImage(context => textField.Layer.RenderInContext(context.CGContext));
			var cgImage = image.CGImage!;
			var width = (int)cgImage.Width;
			var height = (int)cgImage.Height;
			var pixels = new byte[width * height * 4];
			using (var colorSpace = CGColorSpace.CreateDeviceRGB())
			using (var bitmap = new CGBitmapContext(pixels, width, height, 8, width * 4, colorSpace, CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast))
				bitmap.DrawImage(new CGRect(0, 0, width, height), cgImage);

			// The text is red; the caret and the border aren't.
			bool IsText(int x, int y)
			{
				var i = (y * width + x) * 4;
				return pixels[i + 3] > 128 && pixels[i] > 128 && pixels[i + 1] < 100 && pixels[i + 2] < 100;
			}

			var glyphs = new System.Text.StringBuilder();
			var x0 = -1;
			int top = height, bottom = -1, count = 0;
			for (var x = 0; x <= width; x++)
			{
				var columnHasText = false;
				for (var y = 0; x < width && y < height; y++)
				{
					if (!IsText(x, y))
						continue;
					columnHasText = true;
					top = Math.Min(top, y);
					bottom = Math.Max(bottom, y);
					count++;
				}

				if (columnHasText && x0 < 0)
					x0 = x;
				else if (!columnHasText && x0 >= 0)
				{
					// A filled dot is about as wide as it is tall and covers about π/4 of its box.
					double w = x - x0, h = bottom - top + 1;
					var isDot = w / h is > 0.8 and < 1.25 && count / (w * h) > 0.65;
					glyphs.Append(isDot ? '•' : '?');
					x0 = -1;
					top = height;
					bottom = -1;
					count = 0;
				}
			}

			return glyphs.ToString();
		}

		async Task ScrollHelper(Func<Task> func, params StubBase[] views)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler<VerticalStackLayoutStub, LayoutHandler>();
					handler.AddHandler<EntryStub, EntryHandler>();
					handler.AddHandler<EditorStub, EditorHandler>();
				});
			});

			var layout = new VerticalStackLayoutStub();

			foreach (var view in views)
			{
				layout.Add(view);
			}

			layout.Width = 300;
			layout.Height = 800;

			await InvokeOnMainThreadAsync(async () =>
			{
				var contentViewHandler = CreateHandler<LayoutHandler>(layout);
				var contentPlatformView = contentViewHandler.PlatformView;

				await contentPlatformView.AttachAndRun(async () => await func.Invoke());
			});
		}

		async Task ScrollToText(StubBase selectedStub)
		{
			var uiTextField = selectedStub.ToPlatform();
			Assert.True(uiTextField.BecomeFirstResponder());

			var isKeyboardShowing = await Wait(() => KeyboardAutoManagerScroll.IsKeyboardShowing, 1000);

			// on an iOS simulator that has softKeyboard toggled off, we will not see the keyboard
			if (isKeyboardShowing)
			{
				var cursorRect = KeyboardAutoManagerScroll.FindCursorPosition();
				var keyboardHeight = KeyboardAutoManagerScroll.FindKeyboardHeight();

				if (cursorRect is CGRect rect)
					Assert.True(rect.Y < keyboardHeight, "cursor position");
				else
					Assert.Fail("CursorRect should not be null");

				uiTextField.ResignFirstResponder();
				await uiTextField.WaitForKeyboardToHide();
			}
		}

		async Task ScrollToNext(StubBase originalStub, StubBase nextStub)
		{
			var originalUIText = originalStub.ToPlatform();
			var nextUIText = nextStub.ToPlatform();

			KeyboardAutoManager.GoToNextResponderOrResign(originalUIText, customSuperView: originalUIText.Superview);

			Assert.True(nextUIText.BecomeFirstResponder());

			var isKeyboardShowing = await Wait(() => KeyboardAutoManagerScroll.IsKeyboardShowing, 1000);

			// on an iOS simulator that has softKeyboard toggled off, we will not see the keyboard
			if (isKeyboardShowing)
			{
				var cursorRect = KeyboardAutoManagerScroll.FindCursorPosition();
				var keyboardHeight = KeyboardAutoManagerScroll.FindKeyboardHeight();

				if (cursorRect is CGRect rect)
					Assert.True(rect.Y < keyboardHeight, "cursor position");
				else
					Assert.Fail("CursorRect should not be null");

				nextUIText.ResignFirstResponder();
				await nextUIText.WaitForKeyboardToHide();
			}
		}

		double GetNativeCharacterSpacing(EntryHandler entryHandler)
		{
			var entry = GetNativeEntry(entryHandler);
			return entry.AttributedText.GetCharacterSpacing();
		}

		static UITextField GetNativeEntry(EntryHandler entryHandler) =>
			(UITextField)entryHandler.PlatformView;

		static string GetNativeText(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Text;

		internal static void SetNativeText(EntryHandler entryHandler, string text) =>
			GetNativeEntry(entryHandler).Text = text;

		internal static int GetCursorStartPosition(EntryHandler entryHandler)
		{
			var control = GetNativeEntry(entryHandler);
			return (int)control.GetOffsetFromPosition(control.BeginningOfDocument, control.SelectedTextRange.Start);
		}

		internal static void UpdateCursorStartPosition(EntryHandler entryHandler, int position)
		{
			var control = GetNativeEntry(entryHandler);
			var endPosition = control.GetPosition(control.BeginningOfDocument, position);
			control.SelectedTextRange = control.GetTextRange(endPosition, endPosition);
		}

		Color GetNativeTextColor(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).TextColor.ToColor();

		bool GetNativeIsPassword(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).SecureTextEntry;

		string GetNativePlaceholder(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Placeholder;

		bool GetNativeIsTextPredictionEnabled(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).AutocorrectionType == UITextAutocorrectionType.Yes;

		bool GetNativeIsSpellCheckEnabled(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).SpellCheckingType == UITextSpellCheckingType.Yes;

		bool GetNativeIsReadOnly(EntryHandler entryHandler) =>
			!GetNativeEntry(entryHandler).UserInteractionEnabled;

		bool GetNativeIsNumericKeyboard(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).KeyboardType == UIKeyboardType.DecimalPad;

		bool GetNativeIsEmailKeyboard(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).KeyboardType == UIKeyboardType.EmailAddress;

		bool GetNativeIsTelephoneKeyboard(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).KeyboardType == UIKeyboardType.PhonePad;

		bool GetNativeIsUrlKeyboard(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).KeyboardType == UIKeyboardType.Url;

		bool GetNativeIsTextKeyboard(EntryHandler entryHandler)
		{
			var nativeEntry = GetNativeEntry(entryHandler);

			return nativeEntry.AutocapitalizationType == UITextAutocapitalizationType.Sentences &&
				nativeEntry.AutocorrectionType == UITextAutocorrectionType.Yes &&
				nativeEntry.SpellCheckingType == UITextSpellCheckingType.Yes;
		}

		bool GetNativeIsChatKeyboard(EntryHandler entryHandler)
		{
			var nativeEntry = GetNativeEntry(entryHandler);

			return nativeEntry.AutocapitalizationType == UITextAutocapitalizationType.Sentences &&
				nativeEntry.AutocorrectionType == UITextAutocorrectionType.Yes &&
				nativeEntry.SpellCheckingType == UITextSpellCheckingType.Yes;
		}

		bool GetNativeClearButtonVisibility(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).ClearButtonMode == UITextFieldViewMode.WhileEditing;

		static UIButton GetNativeClearButton(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).ValueForKey(new NSString("clearButton")) as UIButton;

		static byte GetMaxAlpha(UIImage image)
		{
			const int bitsPerComponent = 8;
			const int bytesPerPixel = 4; // R, G, B, A (CGImageAlphaInfo.PremultipliedLast)
			const int alphaByteOffset = 3; // A is the 4th byte within each RGBA pixel

			var cgImage = image.CGImage;
			Assert.NotNull(cgImage);

			var width = (int)cgImage.Width;
			var height = (int)cgImage.Height;
			var bytesPerRow = width * bytesPerPixel;
			var pixels = new byte[height * bytesPerRow];

			using var colorSpace = CGColorSpace.CreateDeviceRGB();
			using (var context = new CGBitmapContext(pixels, width, height, bitsPerComponent, bytesPerRow, colorSpace, CGImageAlphaInfo.PremultipliedLast))
			{
				context.DrawImage(new CGRect(0, 0, width, height), cgImage);
			}

			byte maxAlpha = 0;
			for (int i = alphaByteOffset; i < pixels.Length; i += bytesPerPixel)
			{
				if (pixels[i] == byte.MaxValue)
				{
					return byte.MaxValue;
				}

				if (pixels[i] > maxAlpha)
				{
					maxAlpha = pixels[i];
				}
			}

			return maxAlpha;
		}

		UITextAlignment GetNativeHorizontalTextAlignment(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).TextAlignment;

		UIControlContentVerticalAlignment GetNativeVerticalTextAlignment(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).VerticalAlignment;

		UIControlContentVerticalAlignment GetNativeVerticalTextAlignment(TextAlignment textAlignment) =>
			textAlignment.ToPlatformVertical();

		UIReturnKeyType GetNativeReturnType(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).ReturnKeyType;

		int GetNativeCursorPosition(EntryHandler entryHandler)
		{
			var textField = GetNativeEntry(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.BeginningOfDocument, textField.SelectedTextRange.Start);

			return -1;
		}

		int GetNativeSelectionLength(EntryHandler entryHandler)
		{
			var textField = GetNativeEntry(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.SelectedTextRange.Start, textField.SelectedTextRange.End);

			return -1;
		}
	}
}