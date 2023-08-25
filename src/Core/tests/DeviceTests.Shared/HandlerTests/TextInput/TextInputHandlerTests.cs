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
		[Theory(DisplayName = "TextChanged Events Fire Correctly"
#if WINDOWS
			, Skip = "For some reason, the PlatformView.TextChanged event is not being fired on tests, something is swallowing the event firing. " +
					 "This was tested on a real app and it's working correctly."
#endif
			)]
		// null/empty
		[InlineData(null, null, false)]
		[InlineData(null, "", false)]
		[InlineData("", null, false)]
		[InlineData("", "", false)]
		// whitespace
		[InlineData(null, " ", true)]
		[InlineData("", " ", true)]
		[InlineData(" ", null, true)]
		[InlineData(" ", "", true)]
		[InlineData(" ", " ", false)]
		// text
		[InlineData(null, "Hello", true)]
		[InlineData("", "Hello", true)]
		[InlineData(" ", "Hello", true)]
		[InlineData("Hello", null, true)]
		[InlineData("Hello", "", true)]
		[InlineData("Hello", " ", true)]
		[InlineData("Hello", "Goodbye", true)]
		public async Task TextChangedEventsFireCorrectly(string initialText, string newText, bool eventExpected)
		{
			var textInput = new TStub();
			textInput.Text = initialText;

			var eventFiredCount = 0;
			textInput.TextChanged += (sender, e) =>
			{
				eventFiredCount++;

				Assert.Equal(initialText, e.OldValue);
				Assert.Equal(newText ?? string.Empty, e.NewValue);
			};

			await SetValueAsync<string, THandler>(textInput, newText, SetNativeText);

			if (eventExpected)
				Assert.Equal(1, eventFiredCount);
			else
				Assert.Equal(0, eventFiredCount);
		}

		[Fact
#if WINDOWS
			(Skip = "Failing on Windows")
#endif
			]
		public async Task CursorPositionDoesntResetWhenNativeTextValueChanges()
		{
			var textInput = new TStub()
			{
				Text = "Hello"
			};


			int cursorPosition = 0;
			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<THandler>(textInput);
				UpdateCursorStartPosition(handler, 5);
				handler.UpdateValue(nameof(ITextInput.Text));
				cursorPosition = GetCursorStartPosition(handler);
			});

			Assert.Equal(5, cursorPosition);
		}

#if ANDROID
		// This test will only run if the Android Manifest of the Controls.DeviceTests project is edited to have android:supportsRtl="false"
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

		// This test will only run if the Android Manifest of the Controls.DeviceTests project is edited to have android:supportsRtl="false"
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
#endif

		protected abstract void UpdateCursorStartPosition(THandler entryHandler, int position);
		protected abstract int GetCursorStartPosition(THandler entryHandler);
		protected abstract void SetNativeText(THandler entryHandler, string text);
	}
}
