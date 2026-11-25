using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelHandlerTests
	{
		[Theory]
		[Category(TestCategory.Fonts)]
		[InlineData(true, Skip = "https://github.com/dotnet/maui/issues/26749")]
		[InlineData(false)]
		public async Task FontFamilyIsCorrectForRendering(bool validateSize)
		{
			var label = new LabelStub
			{
				Font = Font.OfSize("Ion", 20),
				Text = "\xf30c",
			};

			var (bounds, font) = await GetValueAsync(label, handler =>
			{
				var fe = handler.VirtualView.ToPlatform();
				var tb = fe as TextBlock;

				return tb.AttachAndRun(() => (tb.GetBoundingBox(), tb.FontFamily.Source), MauiContext);
			});

			var expectedFont = "ms-appx:///ionicons.ttf#Ionicons";

			Assert.Equal(expectedFont, font);

			if (validateSize)
			{
				Assert.True(bounds.Width > 20, $"Width ({bounds.Width}x{bounds.Height}) was too narrow, the font probably did not load correctly.");
				Assert.True(bounds.Height < 30, $"Height ({bounds.Width}x{bounds.Height}) was too tall, the font probably did not load correctly.");
			}
		}

		[Theory]
		[Category(TestCategory.Fonts)]
		[InlineData(true, Skip = "https://github.com/dotnet/maui/issues/26749")]
		[InlineData(false)]
		public async Task EmbeddedFontFamilyIsCorrectForRendering(bool validateSize)
		{
			var label = new LabelStub
			{
				Font = Font.OfSize("Embedded", 20),
				Text = "\xf30c",
			};

			var (bounds, font) = await GetValueAsync(label, handler =>
			{
				var fe = handler.VirtualView.ToPlatform();
				var tb = fe as TextBlock;

				return tb.AttachAndRun(() => (tb.GetBoundingBox(), tb.FontFamily.Source), MauiContext);
			});


#if UNPACKAGED
			var expectedSuffix = "Fonts\\ionicons.ttf#Ionicons";
			var root = Path.GetFullPath(Path.Combine(FileSystem.CacheDirectory, ".."));
			var fullFontName = Path.GetFullPath(Path.Combine(root, expectedSuffix));
			var expectedFont = "ms-appx:///" + fullFontName.Replace("\\", "/", StringComparison.OrdinalIgnoreCase);
#else
			var expectedFont = "ms-appdata:///temp/Fonts/ionicons.ttf#Ionicons";
#endif

			Assert.Equal(expectedFont, font);

			if (validateSize)
			{
				Assert.True(bounds.Width > 20, $"Width ({bounds.Width}x{bounds.Height}) was too narrow, the font probably did not load correctly.");
				Assert.True(bounds.Height < 30, $"Height ({bounds.Width}x{bounds.Height}) was too tall, the font probably did not load correctly.");
			}
		}

		[Fact(DisplayName = "LineHeight Resets To Default")]
		public async Task LineHeightResetsToDefault()
		{
			var label = new LabelStub
			{
				Text = "Test",
				LineHeight = 3,
			};

			var handler = await CreateHandlerAsync(label);

			var values = await InvokeOnMainThreadAsync(() =>
			{
				var initial = handler.PlatformView.ReadLocalValue(TextBlock.LineHeightProperty);

				label.LineHeight = -1;
				handler.UpdateValue(nameof(ILabel.LineHeight));

				var reset = handler.PlatformView.ReadLocalValue(TextBlock.LineHeightProperty);
				return (Initial: initial, Reset: reset);
			});

			Assert.NotEqual(DependencyProperty.UnsetValue, values.Initial);
			Assert.Equal(DependencyProperty.UnsetValue, values.Reset);
		}

		bool GetNativeIsTextSelectable(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).IsTextSelectionEnabled;

		[Fact(DisplayName = "IsEnabled Changes Update Text Selection")]
		public async Task IsEnabledChangesUpdateTextSelection()
		{
			var label = new LabelStub { Text = "Test", IsTextSelectable = true };

			var handler = await CreateHandlerAsync(label);

			var values = await InvokeOnMainThreadAsync(() =>
			{
				var initial = GetNativeIsTextSelectable(handler);

				label.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));
				var disabled = GetNativeIsTextSelectable(handler);

				label.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));
				var enabled = GetNativeIsTextSelectable(handler);

				label.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));
				var disabledAgain = GetNativeIsTextSelectable(handler);

				return (initial, disabled, enabled, disabledAgain);
			});

			Assert.Equal((true, false, true, false), values);
		}

		[Fact(DisplayName = "IsEnabled Mapping Appended To ViewMapper Still Runs For Label")]
		public async Task AppendedIsEnabledMappingStillRunsForLabel()
		{
			var label = new LabelStub { Text = "Test", IsTextSelectable = true };
			var handler = await CreateHandlerAsync(label);

			// The mapper is shared and can't be restored, so the callback is switched off again afterwards.
			var active = true;
			var calls = 0;
			ViewHandler.ViewMapper.AppendToMapping<IView, IViewHandler>(nameof(IView.IsEnabled), (h, v) =>
			{
				if (active)
				{
					calls++;
				}
			});

			try
			{
				var selectable = await InvokeOnMainThreadAsync(() =>
				{
					label.IsEnabled = false;
					handler.UpdateValue(nameof(IView.IsEnabled));
					return GetNativeIsTextSelectable(handler);
				});

				Assert.Equal(1, calls);
				Assert.False(selectable);
			}
			finally
			{
				active = false;
			}
		}

		[Fact(DisplayName = "Unset IsTextSelectable Keeps Native Selection")]
		public async Task UnsetIsTextSelectableKeepsNativeSelection()
		{
			var label = new LabelStub { Text = "Test" };
			var handler = await CreateHandlerAsync(label);

			var isSelectable = await InvokeOnMainThreadAsync(() =>
			{
				GetPlatformLabel(handler).IsTextSelectionEnabled = true;
				handler.UpdateValue(nameof(ILabel.IsTextSelectable));
				return GetNativeIsTextSelectable(handler);
			});

			Assert.True(isSelectable);
		}

		TextBlock GetPlatformLabel(LabelHandler labelHandler) =>
			labelHandler.PlatformView;

		string GetNativeText(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).Text;

		Color GetNativeTextColor(LabelHandler labelHandler)
		{
			var platformLabel = GetPlatformLabel(labelHandler);

			var foreground = platformLabel.Foreground;

			if (foreground is SolidColorBrush solidColorBrush)
				return solidColorBrush.Color.ToColor();

			return null;
		}

		UI.Xaml.TextAlignment GetNativeHorizontalTextAlignment(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).TextAlignment;
	}
}