#if IOS || MACCATALYST
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.DeviceTests.Stubs;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ToolbarTests
	{
		[Theory(DisplayName = "Navigation and Shell Toolbar Items Use Platform Metadata")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task NavigationAndShellToolbarItemsUsePlatformMetadata(bool useShell)
		{
			SetupBuilder();
			var toolbarItem = new ToolbarItem
			{
				AutomationId = "DuoToolbarItem",
				IconImageSource = "red.png",
				Text = "Toolbar Item"
			};
			var page = new ContentPage
			{
				ToolbarItems =
				{
					toolbarItem
				}
			};
			Page rootPage = useShell
				? new Shell { CurrentItem = page, FlyoutBehavior = FlyoutBehavior.Disabled }
				: new NavigationPage(page);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(rootPage), async handler =>
			{
				await OnLoadedAsync(page);
				var navigationItem = GetPlatformToolbar(handler).TopItem;
				var nativeItem = Assert.Single(navigationItem.RightBarButtonItems);

				await AssertEventually(() => nativeItem.Image is not null);
				Assert.Equal(
					OperatingSystem.IsIOSVersionAtLeast(27, 1) && !OperatingSystem.IsMacCatalyst()
						? toolbarItem.Text
						: null,
					nativeItem.Title);
				Assert.Equal(toolbarItem.AutomationId, nativeItem.AccessibilityIdentifier);
			});
		}

		[Theory(DisplayName = "Primary Toolbar Item Metadata Follows Version Policy")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task PrimaryToolbarItemMetadataFollowsVersionPolicy(bool useTitleAndImage)
		{
			SetupBuilder();
			var toolbarItem = new ToolbarItem
			{
				AutomationId = "DuoToolbarItem",
				IconImageSource = "red.png",
				IsEnabled = true,
				Text = "Toolbar Item"
			};
			var page = new ContentPage
			{
				ToolbarItems =
				{
					toolbarItem
				}
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(page), async _ =>
			{
				using var nativeItem = toolbarItem.ToUIBarButtonItem(false, false, useTitleAndImage);

				await AssertEventually(() => nativeItem.Image is not null);
				Assert.Equal(useTitleAndImage ? toolbarItem.Text : null, nativeItem.Title);
				Assert.Equal(UIBarButtonItemStyle.Plain, nativeItem.Style);
				Assert.True(nativeItem.Enabled);
				Assert.Equal(toolbarItem.AutomationId, nativeItem.AccessibilityIdentifier);
				Assert.Equal(toolbarItem.Text, nativeItem.AccessibilityLabel);

				toolbarItem.Text = "Updated";
				toolbarItem.IsEnabled = false;

				Assert.Equal(useTitleAndImage ? toolbarItem.Text : null, nativeItem.Title);
				Assert.False(nativeItem.Enabled);

				toolbarItem.IconImageSource = null;

				Assert.Null(nativeItem.Image);
				Assert.Equal(toolbarItem.Text, nativeItem.Title);
			});
		}

		[Fact(DisplayName = "Legacy Toolbar Item Keeps Text When Icon Load Fails")]
		public async Task LegacyToolbarItemKeepsTextWhenIconLoadFails()
		{
			SetupBuilder();
			var imageSource = new ControlledToolbarImageSource();
			var toolbarItem = new ToolbarItem
			{
				Text = "Toolbar Item"
			};
			var page = new ContentPage
			{
				ToolbarItems =
				{
					toolbarItem
				}
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(page), async _ =>
			{
				using var nativeItem = toolbarItem.ToUIBarButtonItem(false, false, false);
				Assert.Equal(toolbarItem.Text, nativeItem.Title);

				toolbarItem.IconImageSource = imageSource;
				await imageSource.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
				imageSource.Complete(null);

				await AssertEventually(() => imageSource.Disposed.Task.IsCompleted);
				Assert.Equal(toolbarItem.Text, nativeItem.Title);
				Assert.Null(nativeItem.Image);
			});
		}

		[Fact(DisplayName = "Primary Toolbar Item Ignores Stale Image Loads")]
		public async Task PrimaryToolbarItemIgnoresStaleImageLoads()
		{
			SetupBuilder();
			var firstSource = new ControlledToolbarImageSource();
			var secondSource = new ControlledToolbarImageSource();
			var toolbarItem = new ToolbarItem
			{
				IconImageSource = firstSource,
				Text = "Toolbar Item"
			};
			var page = new ContentPage
			{
				ToolbarItems =
				{
					toolbarItem
				}
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(page), async _ =>
			{
				using var nativeItem = toolbarItem.ToUIBarButtonItem(false, false, true);
				await firstSource.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));

				toolbarItem.IconImageSource = secondSource;
				await secondSource.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));

				var secondImage = new UIImage();
				secondSource.Complete(secondImage);
				await AssertEventually(() => ReferenceEquals(secondImage, nativeItem.Image));

				var firstImage = new UIImage();
				firstSource.Complete(firstImage);
				await firstSource.Disposed.Task.WaitAsync(TimeSpan.FromSeconds(5));

				Assert.Same(secondImage, nativeItem.Image);
			});

			await secondSource.Disposed.Task.WaitAsync(TimeSpan.FromSeconds(5));
		}

		[Fact(DisplayName = "Disposed Primary Toolbar Item Releases Late Image")]
		public async Task DisposedPrimaryToolbarItemReleasesLateImage()
		{
			SetupBuilder();
			var imageSource = new ControlledToolbarImageSource();
			var toolbarItem = new ToolbarItem
			{
				IconImageSource = imageSource,
				Text = "Toolbar Item"
			};
			var page = new ContentPage
			{
				ToolbarItems =
				{
					toolbarItem
				}
			};

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(page), async _ =>
			{
				var nativeItem = toolbarItem.ToUIBarButtonItem(false, false, true);
				await imageSource.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
				nativeItem.Dispose();

				imageSource.Complete(new UIImage());
				await imageSource.Disposed.Task.WaitAsync(TimeSpan.FromSeconds(5));
			});
		}

		[Theory(DisplayName = "Navigation and Shell Secondary Toolbar Items Keep Menu Metadata")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task NavigationAndShellSecondaryToolbarItemsKeepMenuMetadata(bool useShell)
		{
			SetupBuilder();
			var toolbarItem = new ToolbarItem
			{
				AutomationId = "SecondaryDuoToolbarItem",
				IconImageSource = "red.png",
				IsEnabled = true,
				Order = ToolbarItemOrder.Secondary,
				Text = "Secondary Item"
			};
			var page = new ContentPage
			{
				ToolbarItems =
				{
					toolbarItem
				}
			};
			Page rootPage = useShell
				? new Shell { CurrentItem = page, FlyoutBehavior = FlyoutBehavior.Disabled }
				: new NavigationPage(page);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(rootPage), async handler =>
			{
				await OnLoadedAsync(page);
				var navigationItem = GetPlatformToolbar(handler).TopItem;
				var action = GetSecondaryAction();

				Assert.Equal(toolbarItem.Text, action.Title);
				Assert.NotNull(action.Image);
				Assert.Equal(toolbarItem.AutomationId, action.AccessibilityIdentifier);
				Assert.False(action.Attributes.HasFlag(UIMenuElementAttributes.Disabled));

				toolbarItem.Text = "Updated";
				toolbarItem.IconImageSource = null;
				toolbarItem.IsEnabled = false;

				await AssertEventually(() =>
				{
					action = GetSecondaryAction();
					return action.Title == toolbarItem.Text &&
						action.Image is null &&
						action.Attributes.HasFlag(UIMenuElementAttributes.Disabled);
				});

				Assert.Equal(toolbarItem.Text, action.Title);
				Assert.Null(action.Image);
				Assert.True(action.Attributes.HasFlag(UIMenuElementAttributes.Disabled));

				UIAction GetSecondaryAction()
				{
					var menuButton = Assert.Single(navigationItem.RightBarButtonItems);
					var menu = useShell
						? Assert.IsType<UIButton>(menuButton.CustomView).Menu
						: menuButton.Menu;

					Assert.NotNull(menu);
					return Assert.IsType<UIAction>(Assert.Single(menu.Children));
				}
			});
		}

		interface IControlledToolbarImageSource : IImageSource
		{
		}

		sealed class ControlledToolbarImageSource : ImageSource, IControlledToolbarImageSource
		{
			readonly TaskCompletionSource<IImageSourceServiceResult<UIImage>> _completion =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public TaskCompletionSource<bool> Disposed { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public TaskCompletionSource<bool> Started { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public Task<IImageSourceServiceResult<UIImage>> Result => _completion.Task;

			public void Complete(UIImage image)
			{
				_completion.TrySetResult(new ImageSourceServiceResult(image, () => Disposed.TrySetResult(true)));
			}
		}

		sealed class ControlledToolbarImageSourceService : IImageSourceService<IControlledToolbarImageSource>
		{
			public Task<IImageSourceServiceResult<UIImage>> GetImageAsync(
				IImageSource imageSource,
				float scale = 1,
				CancellationToken cancellationToken = default) =>
				GetImageAsync((IControlledToolbarImageSource)imageSource, scale, cancellationToken);

			public Task<IImageSourceServiceResult<UIImage>> GetImageAsync(
				IControlledToolbarImageSource imageSource,
				float scale = 1,
				CancellationToken cancellationToken = default)
			{
				var source = Assert.IsType<ControlledToolbarImageSource>(imageSource);
				source.Started.TrySetResult(true);
				return source.Result;
			}
		}
	}
}
#endif
