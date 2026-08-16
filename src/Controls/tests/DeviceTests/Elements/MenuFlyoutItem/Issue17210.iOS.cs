#if MACCATALYST
#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.MenuFlyout)]
	public class Issue17210 : ControlsHandlerTestBase
	{
		const string IssueNumber = "17210";

		static string? GetReplicationIssue()
		{
#if ANDROID
			return global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
				.MauiTestInstrumentation.Current?.Arguments?.GetString("MAUI_REPRODUCTION_ISSUE");
#elif IOS || MACCATALYST
			return global::Foundation.NSProcessInfo.ProcessInfo.Environment["MAUI_REPRODUCTION_ISSUE"]?.ToString();
#else
			return Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE");
#endif
		}

		[Fact]
		public async Task BoundIconImageSourceUpdatesNativeMenuIcon()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			try
			{
				await InvokeOnMainThreadAsync(() =>
				{
					var model = new MenuItemModel();
					var item = new MenuFlyoutItem { Text = "Bound menu icon" };
					item.SetBinding(MenuFlyoutItem.IconImageSourceProperty, nameof(MenuItemModel.Icon));
					item.BindingContext = model;

					var handler = CreateHandler<MenuFlyoutItemHandler>(item);
					try
					{
						var nativeItem = Assert.IsType<UICommand>(handler.PlatformView);
						var initialImage = Assert.IsType<UIImage>(nativeItem.Image);

						model.Icon = new FontImageSource
						{
							Glyph = "B",
							Size = 16
						};

						var updatedSource = Assert.IsType<FontImageSource>(item.IconImageSource);
						var updatedImage = Assert.IsType<UIImage>(nativeItem.Image);
						Assert.Equal("B", updatedSource.Glyph);
						Assert.False(
							initialImage.Handle == updatedImage.Handle,
							"The native menu icon should update when its bound IconImageSource changes.");
					}
					finally
					{
						((IElementHandler)handler).DisconnectHandler();
					}
				});
			}
			finally
			{
				MenuFlyoutItemHandler.Reset();
			}
		}

		sealed class MenuItemModel : BindableObject
		{
			ImageSource _icon = new FontImageSource
			{
				Glyph = "A",
				Size = 16
			};

			public ImageSource Icon
			{
				get => _icon;
				set
				{
					if (_icon == value)
						return;

					_icon = value;
					OnPropertyChanged();
				}
			}
		}
	}
}
#endif
