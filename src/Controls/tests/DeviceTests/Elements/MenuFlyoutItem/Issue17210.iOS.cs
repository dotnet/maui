#if MACCATALYST
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.MenuFlyout)]
	public class Issue17210 : ControlsHandlerTestBase
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
