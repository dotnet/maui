﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Entry)]
	public partial class EntryTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
				});
			});
		}

		[Fact(
#if WINDOWS
		Skip = "Fails on Windows"
#endif
		)]
		public async Task MaxLengthTrims()
		{
			var entry = new Entry
			{
				Text = "This is text",
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<EntryHandler>(entry);

				entry.MaxLength = 4;
				Assert.Equal("This", entry.Text);

				var platformText = await GetPlatformText(handler);
				Assert.Equal("This", platformText);
			});
		}

		[Fact(
#if WINDOWS
		Skip = "Fails on Windows"
#endif
		)]
		public async Task InitializingTextTransformBeforeTextShouldUpdateTextProperty()
		{
			var entry = new Entry
			{
				TextTransform = TextTransform.Uppercase,
				Text = "initial text"
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<EntryHandler>(entry);

				Assert.Equal("INITIAL TEXT", entry.Text);
			});
		}

		[Theory(
#if WINDOWS
		Skip = "Fails on Windows"
#endif
		)]
		[InlineData("hello", "HELLO")]
		[InlineData("woRld", "WORLD")]
		public async Task ChangingPlatformTextPreservesTextTransform(string text, string expected)
		{
			var entry = new Entry
			{
				TextTransform = TextTransform.Uppercase,
				Text = "initial text"
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<EntryHandler>(entry);

				SetPlatformText(handler, text);

				Assert.Equal(expected, entry.Text);
			});
		}

#if WINDOWS
		// Only Windows needs the IsReadOnly workaround for MaxLength==0 to prevent text from being entered
		[Fact]
		public async Task MaxLengthIsReadOnlyValueTest()
		{
			Entry entry = new Entry();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<EntryHandler>(entry);
				var platformControl = GetPlatformControl(handler);

				entry.MaxLength = 0;
				Assert.True(platformControl.IsReadOnly);
				entry.IsReadOnly = false;
				Assert.True(platformControl.IsReadOnly);

				entry.MaxLength = 10;
				Assert.False(platformControl.IsReadOnly);
				entry.IsReadOnly = true;
				Assert.True(platformControl.IsReadOnly);
			});
		}


		[Fact(DisplayName = "Unfocus will work when page is shown a 2nd time")]
		public async Task UnFocusOnEntryAfterPagePop()
		{
			int unfocused = 0;
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler(typeof(Window), typeof(WindowHandlerStub));
					handlers.AddHandler(typeof(Entry), typeof(EntryHandler));

				});
			});
			AutoResetEvent _focused = new AutoResetEvent(false);
			AutoResetEvent _unFocused = new AutoResetEvent(false);
			var entry = new Entry();
			entry.Unfocused += (s, e) =>
			{
				if (!e.IsFocused)
				{
					unfocused++;
				}
				_unFocused.Set();
			};
			var navPage = new NavigationPage(new ContentPage { Content = entry });
			var window = new Window(navPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async (handler) =>
			{
				await Task.Run(() =>
				{
					entry.Focused += (s, e) =>
					{
						_focused.Set();
					};

					InvokeOnMainThreadAsync(() =>
					{
						if (!entry.IsFocused)
							entry.Focus();
						else
							_focused.Set();
					});

					_focused.WaitOne();
					_focused.Reset();
					InvokeOnMainThreadAsync(async () =>
					{
						entry.Unfocus();
						await navPage.PushAsync(new ContentPage());
						await navPage.PopAsync();
						entry.Focus();
					});
					_focused.WaitOne();
					_unFocused.Reset();
					InvokeOnMainThreadAsync(() =>
					{
						entry.Unfocus();
					});
					_unFocused.WaitOne();
					Assert.True(unfocused == 2);
				});
			});
		}
#endif

		[Fact]
		[Description("The BackgroundColor of a Entry should match with native background color")]
		public async Task EntryBackgroundColorConsistent()
		{
			var expected = Colors.AliceBlue;
			var entry = new Entry()
			{
				BackgroundColor = expected,
				HeightRequest = 100,
				WidthRequest = 200
			};

			await ValidateHasColor(entry, expected, typeof(EntryHandler));
		}

		[Fact]
		[Description("The Opacity property of an Entry should match with native Opacity")]
		public async Task VerifyEntryOpacityProperty()
		{
			var entry = new Entry
			{
				Opacity = 0.35f
			};
			var expectedValue = entry.Opacity;

			var handler = await CreateHandlerAsync<EntryHandler>(entry);
			await InvokeOnMainThreadAsync(async () =>
			{
				var nativeOpacityValue = await GetPlatformOpacity(handler);
				Assert.Equal(expectedValue, nativeOpacityValue);
			});
		}

		[Category(TestCategory.Entry)]
		[Category(TestCategory.TextInput)]
		[Collection(RunInNewWindowCollection)]
		public class EntryTextInputTests : TextInputTests<EntryHandler, Entry>
		{
			protected override int GetPlatformSelectionLength(EntryHandler handler) =>
				EntryTests.GetPlatformSelectionLength(handler);

			protected override int GetPlatformCursorPosition(EntryHandler handler) =>
				EntryTests.GetPlatformCursorPosition(handler);

			protected override Task<string> GetPlatformText(EntryHandler handler) =>
				EntryTests.GetPlatformText(handler);
		}
	}
}
