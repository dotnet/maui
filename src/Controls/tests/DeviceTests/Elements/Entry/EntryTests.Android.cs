using System.ComponentModel;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		static AppCompatEditText GetPlatformControl(EntryHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		static void SetPlatformText(EntryHandler entryHandler, string text) =>
			GetPlatformControl(entryHandler).SetTextKeepState(text);

		static int GetPlatformCursorPosition(EntryHandler entryHandler)
		{
			var editText = GetPlatformControl(entryHandler);

			if (editText != null)
				return editText.SelectionEnd;

			return -1;
		}

		static int GetPlatformSelectionLength(EntryHandler entryHandler)
		{
			var editText = GetPlatformControl(entryHandler);

			if (editText != null)
				return editText.SelectionEnd - editText.SelectionStart;

			return -1;
		}

		Task<float> GetPlatformOpacity(EntryHandler entryHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(entryHandler);
				return nativeView.Alpha;
			});
		}

		Task<bool> GetPlatformIsVisible(EntryHandler entryHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(entryHandler);
				return nativeView.Visibility == global::Android.Views.ViewStates.Visible;
			});
		}

		[Fact]
		public async Task CursorPositionPreservedWhenTextTransformPresent()
		{
			var entry = new Entry
			{
				Text = "TET",
				TextTransform = TextTransform.Uppercase
			};

			await SetValueAsync<int, EntryHandler>(entry, 2, (h, s) => h.PlatformView.SetSelection(2));

			Assert.Equal(2, entry.CursorPosition);

			await SetValueAsync<string, EntryHandler>(entry, "TEsT", SetPlatformText);

			Assert.Equal(2, entry.CursorPosition);
		}

		[Fact]
		[Category(TestCategory.Entry)]
		public async Task UpdateTextWithTextLongerThanMaxLength()
		{
			string longText = "A text longer than 4 characters";
			var entry = new Entry
			{
				MaxLength = 4,
			};

			await SetValueAsync<string, EntryHandler>(entry, longText, SetPlatformText);

			Assert.Equal(longText[..4], entry.Text);
		}

		[Fact]
		[Description("The ScaleX property of a Entry should match with native ScaleX")]
		public async Task ScaleXConsistent()
		{
			var entry = new Entry() { ScaleX = 0.45f };
			var expected = entry.ScaleX;
			var handler = await CreateHandlerAsync<EntryHandler>(entry);
			var platformEntry = GetPlatformControl(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformEntry.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a Entry should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var entry = new Entry() { ScaleY = 1.23f };
			var expected = entry.ScaleY;
			var handler = await CreateHandlerAsync<EntryHandler>(entry);
			var platformEntry = GetPlatformControl(handler);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformEntry.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a Entry should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var entry = new Entry() { Scale = 2.0f };
			var expected = entry.Scale;
			var handler = await CreateHandlerAsync<EntryHandler>(entry);
			var platformEntry = GetPlatformControl(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformEntry.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformEntry.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a Entry should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var entry = new Entry() { RotationX = 33.0 };
			var expected = entry.RotationX;
			var handler = await CreateHandlerAsync<EntryHandler>(entry);
			var platformEntry = GetPlatformControl(handler);
			var platformRotationX = await InvokeOnMainThreadAsync(() => platformEntry.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a Entry should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var entry = new Entry() { RotationY = 87.0 };
			var expected = entry.RotationY;
			var handler = await CreateHandlerAsync<EntryHandler>(entry);
			var platformEntry = GetPlatformControl(handler);
			var platformRotationY = await InvokeOnMainThreadAsync(() => platformEntry.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a Entry should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var editor = new Entry() { Rotation = 23.0 };
			var expected = editor.Rotation;
			var handler = await CreateHandlerAsync<EntryHandler>(editor);
			var platformEntry = GetPlatformControl(handler);
			var platformRotation = await InvokeOnMainThreadAsync(() => platformEntry.Rotation);
			Assert.Equal(expected, platformRotation);
		}

		//src/Compatibility/Core/tests/Android/TranslationTests.cs
		[Fact]
		[Description("The Translation property of a Entry should match with native Translation")]
		public async Task EntryTranslationConsistent()
		{
			var entry = new Entry()
			{
				Text = "Entry Test",
				TranslationX = 50,
				TranslationY = -20
			};

			var handler = await CreateHandlerAsync<EntryHandler>(entry);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				AssertTranslationMatches(nativeView, entry.TranslationX, entry.TranslationY);
			});
		}
	}
}
