#if WINDOWS
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.MenuFlyout)]
	public partial class Issue17210 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task BoundIconImageSourceUpdatesNativeMenuImage()
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
				var initialIcon = Assert.IsType<Microsoft.UI.Xaml.Controls.FontIcon>(handler.PlatformView.Icon);
				Assert.Equal("A", initialIcon.Glyph);

				viewModel.Icon = CreateIcon("B", Colors.Blue);

				var updatedIcon = Assert.IsType<Microsoft.UI.Xaml.Controls.FontIcon>(handler.PlatformView.Icon);
				Assert.Equal("B", updatedIcon.Glyph);
			});
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
