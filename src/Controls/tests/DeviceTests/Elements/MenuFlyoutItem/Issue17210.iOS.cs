#if MACCATALYST || IOS
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.MenuFlyout)]
	public partial class Issue17210 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task BoundIconImageSourceUpdatesNativeMenuImage()
		{
			try
			{
				await InvokeOnMainThreadAsync(() =>
				{
					var viewModel = new IconViewModel
					{
						Icon = CreateIcon("A", Colors.Red)
					};
					var item = new MenuFlyoutItem { Text = "Bound icon" };
					item.SetBinding(
						MenuItem.IconImageSourceProperty,
						new Binding(nameof(IconViewModel.Icon), source: viewModel));

					var handler = CreateHandler<MenuFlyoutItemHandler>(item);
					var initialImage = handler.PlatformView.Image;
					Assert.NotNull(initialImage);

					viewModel.Icon = CreateIcon("B", Colors.Blue);

					var updatedImage = handler.PlatformView.Image;
					Assert.NotNull(updatedImage);
					Assert.True(
						initialImage.Handle != updatedImage.Handle,
						"The native menu icon should update when the bound IconImageSource changes.");
				});
			}
			finally
			{
				MenuFlyoutItemHandler.Reset();
			}
		}

		[Fact]
		public async Task ValidateCommandCopiesUpdatedIconToSeparateCommand()
		{
			try
			{
				await InvokeOnMainThreadAsync(() =>
				{
					var viewModel = new IconViewModel
					{
						Icon = CreateIcon("A", Colors.Red)
					};
					var item = new MenuFlyoutItem { Text = "Menu bar icon" };
					item.SetBinding(
						MenuItem.IconImageSourceProperty,
						new Binding(nameof(IconViewModel.Icon), source: viewModel));

					var handler = CreateHandler<MenuFlyoutItemHandler>(item);
					var handlerCommand = Assert.IsAssignableFrom<UICommand>(handler.PlatformView);

					// The menu bar validates a separate UICommand instance that only shares
					// the PropertyList index with the handler's command.
					var validatedCommand = UICommand.Create(
						item.Text,
						null,
						new Selector(KeyboardAcceleratorExtensions.MenuItemSelectedSelector),
						handlerCommand.PropertyList);

					viewModel.Icon = CreateIcon("B", Colors.Blue);

					MauiUIApplicationDelegate.Current.ValidateCommand(validatedCommand);

					var expectedImage = handler.PlatformView.Image;
					Assert.NotNull(expectedImage);
					Assert.NotNull(validatedCommand.Image);
					Assert.True(
						expectedImage.Handle == validatedCommand.Image.Handle,
						"ValidateCommand should copy the updated icon onto the validated UICommand.");
				});
			}
			finally
			{
				MenuFlyoutItemHandler.Reset();
			}
		}

		static FontImageSource CreateIcon(string glyph, Color color) =>
			new FontImageSource
			{
				Glyph = glyph,
				Color = color,
				Size = 24
			};

		class IconViewModel : BindableObject
		{
			ImageSource _icon;

			public ImageSource Icon
			{
				get => _icon;
				set
				{
					_icon = value;
					OnPropertyChanged();
				}
			}
		}
	}
}
#endif
